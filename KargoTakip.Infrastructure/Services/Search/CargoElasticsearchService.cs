using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using KargoTakip.Application.Interfaces.Cargo;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services.Search
{
    public class CargoElasticsearchService : ICargoSearchService
    {
        private const string IndexName = "cargos";
        private readonly ElasticsearchClient _client;
        private readonly ILogger<CargoElasticsearchService> _logger;

        public CargoElasticsearchService(ElasticsearchClient client, ILogger<CargoElasticsearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<CargoSearchResult?> SearchIdsAsync(string searchTerm, CargoSearchFilter filter, int skip, int take, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return null;

            var term = searchTerm.Trim();
            try
            {
                var response = await _client.SearchAsync<CargoSearchDocument>(s => s
                    .Indices(IndexName)
                    .From(skip)
                    .Size(take)
                    .Query(q => q.Bool(b => BuildBoolQuery(b, term, filter)))
                    .Sort(sort => sort.Field(f => f.CreatedDate, f => f.Order(SortOrder.Desc))),
                    cancellationToken);

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Elasticsearch arama gecersiz yanit. Hata: {Error}", response.DebugInformation);
                    return null;
                }

                var ids = response.Hits.Select(h => h.Source?.Id ?? h.Id).Where(id => !string.IsNullOrEmpty(id)).ToList();
                var total = response.Total;

                _logger.LogDebug("Elasticsearch arama: {Count} sonuc, toplam {Total}. Term: {Term}", ids.Count, total, term);

                return new CargoSearchResult
                {
                    Ids = ids,
                    TotalCount = (int)total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch arama hatasi. Term: {Term}", term);
                return null;
            }
        }

        private static BoolQueryDescriptor<CargoSearchDocument> BuildBoolQuery(BoolQueryDescriptor<CargoSearchDocument> b, string term, CargoSearchFilter? filter)
        {
            b = b
                .Should(
                    so => so.Term(t => t.Field("trackingNumber").Value(Elastic.Clients.Elasticsearch.FieldValue.String(term))),
                    so => so.MultiMatch(m => m
                        .Fields(new[]
                        {
                            "senderFullName", "senderAddress", "senderPhone",
                            "receiverFullName", "receiverEmail", "receiverAddress", "receiverPhone"
                        })
                        .Query(term)
                        .Type(TextQueryType.BestFields)))
                .MinimumShouldMatch(1);

            if (filter == null) return b;

            if (filter.Status.HasValue)
                b = b.Filter(f => f.Term(t => t.Field("status").Value(Elastic.Clients.Elasticsearch.FieldValue.Long((int)filter.Status.Value))));
            if (filter.StartDate.HasValue)
                b = b.Filter(f => f.Range(r => r.Date(d => d.Field("createdDate").Gte(Elastic.Clients.Elasticsearch.DateMath.Anchored(filter.StartDate.Value)))));
            if (filter.EndDate.HasValue)
                b = b.Filter(f => f.Range(r => r.Date(d => d.Field("createdDate").Lte(Elastic.Clients.Elasticsearch.DateMath.Anchored(filter.EndDate.Value)))));
            if (!string.IsNullOrWhiteSpace(filter.SenderName))
                b = b.Filter(f => f.Match(m => m.Field("senderFullName").Query(filter.SenderName!.Trim())));
            if (!string.IsNullOrWhiteSpace(filter.ReceiverName))
                b = b.Filter(f => f.Match(m => m.Field("receiverFullName").Query(filter.ReceiverName!.Trim())));
            if (!string.IsNullOrWhiteSpace(filter.ReceiverEmail))
                b = b.Filter(f => f.Match(m => m.Field("receiverEmail").Query(filter.ReceiverEmail!.Trim())));

            return b;
        }

        public async Task IndexAsync(CargoSearchDocument document, CancellationToken cancellationToken = default)
        {
            if (document == null || string.IsNullOrEmpty(document.Id))
                return;

            try
            {
                var response = await _client.IndexAsync(document, i => i.Index(IndexName).Id(document.Id), cancellationToken);
                if (!response.IsValidResponse)
                    _logger.LogWarning("Elasticsearch index yazma basarisiz. Id: {Id}, Hata: {Error}", document.Id, response.DebugInformation);
                else
                    _logger.LogDebug("Kargo indexlendi. Id: {Id}, TakipNo: {TrackingNumber}", document.Id, document.TrackingNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch index yazma hatasi. Id: {Id}", document.Id);
            }
        }

        public async Task DeleteAsync(string cargoId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(cargoId))
                return;

            try
            {
                var response = await _client.DeleteAsync(IndexName, cargoId, cancellationToken);
                if (!response.IsValidResponse && response.Result.ToString() != "NotFound")
                    _logger.LogWarning("Elasticsearch silme basarisiz. Id: {Id}", cargoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch silme hatasi. Id: {Id}", cargoId);
            }
        }

    }
}
