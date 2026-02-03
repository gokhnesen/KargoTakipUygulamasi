using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Infrastructure.Services;
using KargoTakip.Persistence.Repositories.CargoMovementRepository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var useBrevo = configuration.GetValue<bool>("Brevo:Enabled", false);

            services.AddMemoryCache();
            services.AddSingleton<ILoginOtpStore, MemoryLoginOtpStore>();
            services.AddScoped<IEmailService, BrevoEmailService>();
            services.AddScoped<ISmsService, DummySmsService>();
            services.AddScoped<INotificationService, NotificationService>();
            
            return services;
        }
    }
}
