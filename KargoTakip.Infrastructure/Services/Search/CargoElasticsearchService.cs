using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services.Search
{
    /// <summary>
    /// SearchTerm aramasini Elasticsearch uzerinden yapar; 600k+ kayitta sub-second arama icin kullanilir.
    /// </summary>
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

        public async Task<CargoSearchResult?> SearchIdsAsync(
            string searchTerm,
            CargoSearchFilter filter,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var searchTermLower = searchTerm.Trim().ToLowerInvariant();
                var must = new List<Query>();

                // MultiMatch: tam metin + benzerlik (isim, adres, email vb.)
                var multiMatch = new MultiMatchQuery
                {
                    Query = searchTermLower,
                    Fields = new[]
                    {
                        "trackingNumber^2",
                        "senderFullName",
                        "senderAddress",
                        "receiverFullName",
                        "receiverEmail",
                        "receiverAddress"
                    },
                    Type = TextQueryType.BestFields,
                    Fuzziness = new Elastic.Clients.Elasticsearch.Fuzziness("AUTO")
                };

                // Tracking number kismi eslesme: "j948" -> "J948CLOR7K" (prefix; text alani lowercase term ile indexlenir)
                var trackingPrefix = new PrefixQuery(new Field("trackingNumber")) { Value = searchTermLower };

                var searchTermQuery = new BoolQuery
                {
                    Should = new Query[]
                    {
                        multiMatch,
                        trackingPrefix
                    },
                    MinimumShouldMatch = 1
                };
                must.Add(searchTermQuery);

                var filters = new List<Query>();

                if (filter.Status.HasValue)
                {
                    filters.Add(new TermQuery(new Field("status")) { Value = (int)filter.Status.Value });
                }

                if (filter.StartDate.HasValue)
                {
                    filters.Add(new DateRangeQuery(new Field("createdDate"))
                    {
                        Gte = filter.StartDate.Value
                    });
                }

                if (filter.EndDate.HasValue)
                {
                    filters.Add(new DateRangeQuery(new Field("createdDate"))
                    {
                        Lte = filter.EndDate.Value
                    });
                }

                if (!string.IsNullOrWhiteSpace(filter.SenderName))
                {
                    var senderName = filter.SenderName.Trim().ToLowerInvariant();
                    filters.Add(new MatchQuery(new Field("senderFullName")) { Query = senderName });
                }

                if (!string.IsNullOrWhiteSpace(filter.ReceiverName))
                {
                    var receiverName = filter.ReceiverName.Trim().ToLowerInvariant();
                    filters.Add(new MatchQuery(new Field("receiverFullName")) { Query = receiverName });
                }

                if (!string.IsNullOrWhiteSpace(filter.ReceiverEmail))
                {
                    var receiverEmail = filter.ReceiverEmail.Trim().ToLowerInvariant();
                    filters.Add(new MatchQuery(new Field("receiverEmail")) { Query = receiverEmail });
                }

                var boolQuery = new BoolQuery
                {
                    Must = must,
                    Filter = filters.Count > 0 ? filters : null
                };

                var response = await _client.SearchAsync<CargoSearchDoc>(s => s
                    .Indices(IndexName)
                    .Query(boolQuery)
                    .From(skip)
                    .Size(take)
                    .Sort(sort => sort.Field(new Field("createdDate"), fs => fs.Order(SortOrder.Desc))),
                    cancellationToken);

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning(response.DebugInformation);
                    return null;
                }

                var ids = response.Hits.Select(h => h.Id!.ToString()).ToList();
                var totalCount = response.Total;

                return new CargoSearchResult
                {
                    Ids = ids,
                    TotalCount = (int)totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch arama hatasi. SearchTerm: {SearchTerm}", searchTerm);
                return null;
            }
        }

        public async Task IndexAsync(CargoSearchDocument document, CancellationToken cancellationToken = default)
        {
            try
            {
                var doc = new CargoSearchDoc
                {
                    Id = document.Id,
                    TrackingNumber = document.TrackingNumber ?? string.Empty,
                    SenderFullName = document.SenderFullName ?? string.Empty,
                    SenderAddress = document.SenderAddress ?? string.Empty,
                    SenderPhone = document.SenderPhone ?? string.Empty,
                    ReceiverFullName = document.ReceiverFullName ?? string.Empty,
                    ReceiverEmail = document.ReceiverEmail ?? string.Empty,
                    ReceiverAddress = document.ReceiverAddress ?? string.Empty,
                    ReceiverPhone = document.ReceiverPhone ?? string.Empty,
                    Status = document.Status,
                    CreatedDate = document.CreatedDate
                };

                var response = await _client.IndexAsync(doc, i => i
                    .Index(IndexName)
                    .Id(document.Id),
                    cancellationToken);

                if (!response.IsValidResponse)
                    _logger.LogWarning("Elasticsearch index hatasi. Id: {Id}, Debug: {Debug}", document.Id, response.DebugInformation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch index hatasi. CargoId: {Id}", document.Id);
                throw new Application.Exceptions.ExternalServiceException("Elasticsearch index hatasi.", ex);
            }
        }

        public async Task DeleteAsync(string cargoId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.DeleteAsync(Elastic.Clients.Elasticsearch.IndexName.From<CargoSearchDoc>(IndexName), Elastic.Clients.Elasticsearch.Id.From(cargoId), cancellationToken);
                if (!response.IsValidResponse && response.Result != Result.NotFound)
                    _logger.LogWarning("Elasticsearch delete uyari. Id: {Id}, Debug: {Debug}", cargoId, response.DebugInformation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch delete hatasi. CargoId: {Id}", cargoId);
            }
        }

        /// <summary>
        /// ES index'teki dokuman sekli (kucuk harfle map edilir).
        /// </summary>
        private class CargoSearchDoc
        {
            public string Id { get; set; } = string.Empty;
            public string TrackingNumber { get; set; } = string.Empty;
            public string SenderFullName { get; set; } = string.Empty;
            public string SenderAddress { get; set; } = string.Empty;
            public string SenderPhone { get; set; } = string.Empty;
            public string ReceiverFullName { get; set; } = string.Empty;
            public string ReceiverEmail { get; set; } = string.Empty;
            public string ReceiverAddress { get; set; } = string.Empty;
            public string ReceiverPhone { get; set; } = string.Empty;
            public int Status { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
}
