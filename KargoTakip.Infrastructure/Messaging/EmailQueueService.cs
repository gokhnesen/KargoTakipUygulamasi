using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Application.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Messaging
{
    public class EmailQueueService : IEmailQueue, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailQueueService> _logger;
        private readonly Lazy<IConnection> _connectionLazy;
        private bool _disposed;

        public EmailQueueService(IConfiguration configuration, ILogger<EmailQueueService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionLazy = new Lazy<IConnection>(CreateConnection);
        }

        private IConnection CreateConnection()
        {
            try
            {
                var factory = RabbitMQConnectionFactory.CreateFromConfiguration(_configuration);
                var connection = factory.CreateConnection();
                _logger.LogInformation("RabbitMQ connection established.");
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RabbitMQ connection.");
                throw;
            }
        }

        public Task PublishAsync(SendEmailMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(message.Email))
                throw new ArgumentException("Email is required.", nameof(message));

            try
            {
                using var channel = _connectionLazy.Value.CreateModel();
                var queueName = _configuration["RabbitMQ:QueueName"] ?? "email-send";
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";

                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
                _logger.LogInformation("Mail RabbitMQ kuyruguna gonderildi. Alici: {Email}, Tip: {MessageType}, Kuyruk: {QueueName}", message.Email, message.MessageType ?? "Unknown", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish email message to queue. Email: {Email}", message.Email);
                throw;
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (_connectionLazy.IsValueCreated)
            {
                try { _connectionLazy.Value.Close(); } catch { /* ignore */ }
                try { _connectionLazy.Value.Dispose(); } catch { /* ignore */ }
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
