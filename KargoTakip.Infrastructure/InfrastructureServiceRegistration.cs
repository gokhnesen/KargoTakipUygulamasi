using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Infrastructure.Services;
using KargoTakip.Persistence.Repositories.CargoMovementRepository;
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
            var useInfobip = configuration.GetValue<bool>("Infobip:Enabled", false);
            var useAzureCommunicationServices = configuration.GetValue<bool>("AzureCommunicationServices:Enabled", false);

            if (useInfobip)
            {
                services.AddHttpClient<ISmsService, InfobipSmsService>();
                services.AddScoped<IEmailService, MockEmailService>();
            }
            else if (useAzureCommunicationServices)
            {
                //services.AddScoped<IEmailService, AzureCommunicationEmailService>();
                //services.AddScoped<ISmsService, AzureCommunicationSmsService>();
            }
            else
            {
                services.AddScoped<IEmailService, MockEmailService>();
                services.AddScoped<ISmsService, MockSmsService>();
            }

            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
