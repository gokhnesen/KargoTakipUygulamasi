using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Specifications.CargoSpecifications;
using KargoTakip.Domain.Entities;
using KargoTakip.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.API.HostedServices
{
    /// <summary>
    /// Uygulama her acilisinda veritabanindaki kargolari Elasticsearch'e arka planda yazar (reindex).
    /// Tek HTTP istegi timeout olmaz; batch'ler halinde ve uzun SQL timeout ile calisir.
    /// Elasticsearch baglantisi yoksa calismaz.
    /// </summary>
    public class ElasticsearchReindexHostedService : BackgroundService
    {
        private const int BatchSize = 200;
        private const int SqlCommandTimeoutSeconds = 120;
        private const int DelayBetweenBatchesMs = 100;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ElasticsearchReindexHostedService> _logger;

        public ElasticsearchReindexHostedService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<ElasticsearchReindexHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var esConnection = _configuration["ConnectionStrings:elasticsearch"]
                ?? _configuration["ConnectionStrings__elasticsearch"];
            if (string.IsNullOrWhiteSpace(esConnection))
            {
                _logger.LogDebug("Elasticsearch baglantisi yok; reindex atlaniyor.");
                return;
            }

            _logger.LogInformation("Elasticsearch reindex arka planda baslatiliyor (batch={BatchSize}, timeout={Timeout}s).", BatchSize, SqlCommandTimeoutSeconds);

            _ = Task.Run(() => ReindexAsync(stoppingToken), stoppingToken);
            await Task.CompletedTask;
        }

        private async Task ReindexAsync(CancellationToken stoppingToken)
        {
            var totalIndexed = 0;
            var page = 1;

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<CargoDbContext>();
                    context.Database.SetCommandTimeout(TimeSpan.FromSeconds(SqlCommandTimeoutSeconds));

                    var repository = scope.ServiceProvider.GetRequiredService<IGenericRepository<Cargo>>();
                    var searchService = scope.ServiceProvider.GetRequiredService<ICargoSearchService>();

                    var filterParams = new CargoFilterParams
                    {
                        PageIndex = page,
                        PageSize = BatchSize,
                        IncludeMovements = false,
                        UseKeysetPagination = false,
                        OrderBy = "date",
                        IsDescending = true
                    };
                    var spec = new CargoFilterSpecification(filterParams);
                    var cargos = await repository.GetAllWithSpecAsync(spec);

                    if (cargos == null || cargos.Count == 0)
                        break;

                    foreach (var c in cargos)
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                        var doc = new CargoSearchDocument
                        {
                            Id = c.Id,
                            TrackingNumber = c.TrackingNumber ?? string.Empty,
                            SenderFullName = c.Sender?.FullName ?? string.Empty,
                            SenderAddress = c.Sender?.FullAddress ?? string.Empty,
                            SenderPhone = c.Sender?.PhoneNumber ?? string.Empty,
                            ReceiverFullName = (c.Receiver != null ? $"{c.Receiver.Name} {c.Receiver.Surname}".Trim() : string.Empty),
                            ReceiverEmail = c.Receiver?.Email ?? string.Empty,
                            ReceiverAddress = c.Receiver?.FullAddress ?? string.Empty,
                            ReceiverPhone = c.Receiver?.PhoneNumber ?? string.Empty,
                            Status = (int)c.Status,
                            CreatedDate = c.CreatedDate
                        };
                        await searchService.IndexAsync(doc, stoppingToken);
                        totalIndexed++;
                    }

                    if (cargos.Count < BatchSize)
                        break;
                    page++;
                    await Task.Delay(DelayBetweenBatchesMs, stoppingToken);
                }

                _logger.LogInformation("Elasticsearch reindex tamamlandi. Toplam {TotalIndexed:N0} kargo indexlendi.", totalIndexed);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Elasticsearch reindex iptal edildi. Indexlenen: {TotalIndexed:N0}.", totalIndexed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch reindex hatasi. Indexlenen: {TotalIndexed:N0}.", totalIndexed);
            }
        }
    }
}
