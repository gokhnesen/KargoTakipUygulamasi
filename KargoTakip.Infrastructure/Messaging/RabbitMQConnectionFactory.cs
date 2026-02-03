using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace KargoTakip.Infrastructure.Messaging
{
    /// <summary>
    /// Creates RabbitMQ connections from configuration.
    /// Supports Aspire WithReference (ConnectionStrings:messaging URI) or RabbitMQ section (HostName, Port, etc.).
    /// </summary>
    public static class RabbitMQConnectionFactory
    {
        public static ConnectionFactory CreateFromConfiguration(IConfiguration configuration)
        {
            var uri = configuration["ConnectionStrings:messaging"];
            if (!string.IsNullOrWhiteSpace(uri))
            {
                return new ConnectionFactory
                {
                    Uri = new Uri(uri),
                    AutomaticRecoveryEnabled = true,
                    DispatchConsumersAsync = true
                };
            }

            var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
            var port = configuration.GetValue<int>("RabbitMQ:Port", 5672);
            var userName = configuration["RabbitMQ:UserName"] ?? "guest";
            var password = configuration["RabbitMQ:Password"] ?? "guest";

            return new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                AutomaticRecoveryEnabled = true,
                DispatchConsumersAsync = true
            };
        }
    }
}
