namespace KargoTakip.Application.Helpers
{
    public static class PaginationHelper
    {
        public const int DefaultPageIndex = 1;
        public const int DefaultPageSize = 50;
        public const int MaxPageSize = 500;

        public static int NormalizePageIndex(int pageIndex)
        {
            return pageIndex < 1 ? DefaultPageIndex : pageIndex;
        }

        public static int NormalizePageSize(int pageSize)
        {
            if (pageSize <= 0) return DefaultPageSize;
            return pageSize > MaxPageSize ? MaxPageSize : pageSize;
        }

        public static int GetSkip(int pageIndex, int pageSize)
        {
            var index = NormalizePageIndex(pageIndex);
            var size = NormalizePageSize(pageSize);
            return (index - 1) * size;
        }
    }
}
