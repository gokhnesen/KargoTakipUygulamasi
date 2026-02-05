using System;

namespace KargoTakip.Application.Helpers
{
    /// <summary>
    /// Sayfalama parametrelerini ve hesaplanmış EffectivePageIndex / EffectivePageSize / Skip içeren temel istek.
    /// </summary>
    public abstract class BasePagedRequest
    {
        public int PageIndex { get; set; } = PaginationHelper.DefaultPageIndex;
        public int PageSize { get; set; } = PaginationHelper.DefaultPageSize;

        /// <summary>
        /// Normalleştirilmiş sayfa indeksi (1'den küçük olamaz).
        /// </summary>
        public int EffectivePageIndex => PaginationHelper.NormalizePageIndex(PageIndex);

        /// <summary>
        /// Normalleştirilmiş sayfa boyutu (0/negatif olamaz, maksimum PaginationHelper.MaxPageSize).
        /// </summary>
        public int EffectivePageSize => PaginationHelper.NormalizePageSize(PageSize);

        /// <summary>
        /// Bu sayfa için atlanacak kayıt sayısı (EffectivePageIndex ve EffectivePageSize ile hesaplanır).
        /// </summary>
        public int Skip => PaginationHelper.GetSkip(EffectivePageIndex, EffectivePageSize);
    }
}
