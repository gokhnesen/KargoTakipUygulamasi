using FluentValidation;
using KargoTakip.Application.Behaviors;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;


namespace KargoTakip.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly));
            services.AddValidatorsFromAssembly(typeof(ApplicationServiceRegistration).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<CargoStatusHelper>();
            services.AddScoped<EmailTemplateBuilder>();
            services.AddScoped<TrackingNumberGenerator>();
            return services;
        }
    }
}
