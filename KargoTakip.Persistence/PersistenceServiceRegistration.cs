using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Persistence.DatabaseContext;
using KargoTakip.Persistence.Repositories;
using KargoTakip.Persistence.Repositories.Cargo;
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

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ICargoRepository, CargoRepository>();

            return services;
        }
    }
}
