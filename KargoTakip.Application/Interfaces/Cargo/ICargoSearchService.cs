using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Cargo
{
    /// <summary>
    /// Search term ile kargo aramasi (Elasticsearch veya DB fallback).
    /// SearchIdsAsync null donerse DB ile arama yapilir.
    /// </summary>
    public interface ICargoSearchService
    {
        /// <summary>
        /// Aramaya uyan kargo ID'lerini doner (sayfalama ile).
        /// ids null ise Elasticsearch kullanilmiyor, DB ile devam edin.
        /// </summary>
        Task<CargoSearchResult?> SearchIdsAsync(
            string searchTerm,
            CargoSearchFilter filter,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kargo dokumanini indexe ekler veya gunceller.
        /// </summary>
        Task IndexAsync(CargoSearchDocument document, CancellationToken cancellationToken = default);

        /// <summary>
        /// Index'ten kargo siler (soft delete senaryosu icin).
        /// </summary>
        Task DeleteAsync(string cargoId, CancellationToken cancellationToken = default);
    }

    public class CargoSearchResult
    {
        public IReadOnlyList<string> Ids { get; set; } = Array.Empty<string>();
        public int TotalCount { get; set; }
    }

    public class CargoSearchFilter
    {
        public CargoStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverEmail { get; set; }
    }

    public class CargoSearchDocument
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
