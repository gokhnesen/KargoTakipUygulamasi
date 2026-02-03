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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
                _logger.LogInformation("Email consumer started. Queue: {QueueName}", queueName);

                await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email consumer failed to start.");
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
                await emailService.SendAsync(message.Email, message.Subject, message.PlainBody, htmlBody).ConfigureAwait(false);

                channel.BasicAck(ea.DeliveryTag, false);
                _logger.LogDebug("Email sent and acked. Type: {MessageType}, To: {Email}", message.MessageType ?? "Unknown", message.Email);
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
