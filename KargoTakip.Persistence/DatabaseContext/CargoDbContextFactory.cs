using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KargoTakip.Persistence.DatabaseContext
{
    /// <summary>
    /// EF Core tasarim zamani (migrations, database update) icin DbContext olusturur.
    /// </summary>
    public class CargoDbContextFactory : IDesignTimeDbContextFactory<CargoDbContext>
    {
        public CargoDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var apiPath = Path.Combine(basePath, "..", "KargoTakip.API");
            if (!Directory.Exists(apiPath))
                apiPath = Path.Combine(basePath, "..", "..", "KargoTakip.API");
            if (!Directory.Exists(apiPath))
                apiPath = basePath;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection bulunamadi. appsettings.json kontrol edin.");

            var optionsBuilder = new DbContextOptionsBuilder<CargoDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new CargoDbContext(optionsBuilder.Options);
        }
    }
}
