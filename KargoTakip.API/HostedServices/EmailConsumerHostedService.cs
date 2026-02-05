using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Application.Messages;
using KargoTakip.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.API.HostedServices
{
    public class EmailConsumerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailConsumerHostedService> _logger;
        private IConnection? _connection;
        private IModel? _channel;

        public EmailConsumerHostedService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<EmailConsumerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        private const int MaxConnectionRetries = 10;
        private const int ConnectionRetryDelayMs = 3000;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (int attempt = 1; attempt <= MaxConnectionRetries; attempt++)
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                try
                {
                    var factory = RabbitMQConnectionFactory.CreateFromConfiguration(_configuration);
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    var queueName = _configuration["RabbitMQ:QueueName"] ?? "email-send";
                    _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.Received += OnMessageReceivedAsync;

                    _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                    _logger.LogInformation("Email consumer baslatildi. Kuyruk: {QueueName}. Mail istekleri kullaniciya gonderilecek.", queueName);

                    await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ baglantisi basarisiz (deneme {Attempt}/{Max}). {Delay}s sonra tekrar denenecek.", attempt, MaxConnectionRetries, ConnectionRetryDelayMs / 1000);
                    if (attempt == MaxConnectionRetries)
                    {
                        _logger.LogError(ex, "Email consumer baslatilamadi. Mail gonderimi calismayacak.");
                        return;
                    }
                    await Task.Delay(ConnectionRetryDelayMs, stoppingToken).ConfigureAwait(false);
                }
            }
        }

        private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
        {
            var channel = ((AsyncEventingBasicConsumer)sender).Model;
            SendEmailMessage? message = null;
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                message = JsonSerializer.Deserialize<SendEmailMessage>(json);

                if (message == null || string.IsNullOrWhiteSpace(message.Email))
                {
                    _logger.LogWarning("Invalid email message received (null or empty email). Nacking.");
                    channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var htmlBody = message.HtmlBody ?? message.PlainBody;
                _logger.LogInformation("Mail kuyruktan alindi, kullaniciya gonderiliyor: {Email}, Tip: {MessageType}", message.Email, message.MessageType ?? "Unknown");
                await emailService.SendAsync(message.Email, message.Subject, message.PlainBody, htmlBody).ConfigureAwait(false);

                channel.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation("Mail kullaniciya gonderildi ve kuyruktan silindi. Alici: {Email}, Tip: {MessageType}", message.Email, message.MessageType ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email message. Nacking (requeue=false). Email: {Email}", message?.Email ?? "(unknown)");
                try
                {
                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
                catch (Exception nackEx)
                {
                    _logger.LogError(nackEx, "Failed to Nack message.");
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_channel != null)
                {
                    _channel.Close();
                    _channel.Dispose();
                }
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing RabbitMQ consumer.");
            }
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
