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

        private const int ConnectionRetrySeconds = 5;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = RabbitMQConnectionFactory.CreateFromConfiguration(_configuration);
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "email-send";

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.Received += OnMessageReceivedAsync;
                    _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

                    _logger.LogInformation("Email consumer baslatildi. Kuyruk: {QueueName}. Mail istekleri kullaniciya gonderilecek.", queueName);
                    await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ baglantisi kurulamadi (Aspire ile RabbitMQ gec basiliyor olabilir). {Seconds} saniye sonra tekrar denenecek.", ConnectionRetrySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(ConnectionRetrySeconds), stoppingToken).ConfigureAwait(false);
                }
            }
        }

        private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
        {
            var channel = ((AsyncEventingBasicConsumer)sender).Model;
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<SendEmailMessage>(json);

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

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
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
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
