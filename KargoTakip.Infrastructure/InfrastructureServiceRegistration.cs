using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Infrastructure.Messaging;
using KargoTakip.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KargoTakip.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var useBrevo = configuration.GetValue<bool>("Brevo:Enabled", false);

            services.AddMemoryCache();
            services.AddSingleton<ILoginOtpStore, MemoryLoginOtpStore>();
            if (useBrevo)
                services.AddScoped<IEmailService, BrevoEmailService>();
            else
                services.AddScoped<IEmailService, DummyEmailService>();
            services.AddScoped<ISmsService, DummySmsService>();
            services.AddSingleton<IEmailQueue, EmailQueueService>();

            return services;
        }
    }
}
