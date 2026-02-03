using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Interfaces.CargoMovement;
using KargoTakip.Domain.Entities;
using KargoTakip.Persistence.DatabaseContext;
using KargoTakip.Persistence.Repositories;
using KargoTakip.Persistence.Repositories.Cargo;
using KargoTakip.Persistence.Repositories.CargoMovementRepository;
using KargoTakip.Persistence.Services;
using KargoTakip.Persistence.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CargoDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<CargoDbContext>()
                .AddDefaultTokenProviders();


            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ICargoRepository, CargoRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICargoMovementRepository, CargoMovementRepository>();
            services.AddScoped<DataSeeder>();

            return services;
        }
    }
}
