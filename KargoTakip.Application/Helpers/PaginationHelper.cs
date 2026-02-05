namespace KargoTakip.Application.Helpers
{
    /// <summary>
    /// Sayfalama indeks ve boyut hesaplamalarını merkezileştirir.
    /// </summary>
    public static class PaginationHelper
    {
        public const int DefaultPageIndex = 1;
        public const int DefaultPageSize = 50;
        public const int MaxPageSize = 500;

        /// <summary>
        /// Geçerli sayfa indeksini döner (1'den küçükse 1, aksi halde değerin kendisi).
        /// </summary>
        public static int NormalizePageIndex(int pageIndex)
        {
            return pageIndex < 1 ? DefaultPageIndex : pageIndex;
        }

        /// <summary>
        /// Geçerli sayfa boyutunu döner (0 veya negatifse DefaultPageSize, MaxPageSize'dan büyükse MaxPageSize).
        /// </summary>
        public static int NormalizePageSize(int pageSize)
        {
            if (pageSize <= 0) return DefaultPageSize;
            return pageSize > MaxPageSize ? MaxPageSize : pageSize;
        }

        /// <summary>
        /// Verilen sayfa indeks ve boyutuna göre atlanacak kayıt sayısını (skip) döner.
        /// </summary>
        public static int GetSkip(int pageIndex, int pageSize)
        {
            var index = NormalizePageIndex(pageIndex);
            var size = NormalizePageSize(pageSize);
            return (index - 1) * size;
        }
    }
}
