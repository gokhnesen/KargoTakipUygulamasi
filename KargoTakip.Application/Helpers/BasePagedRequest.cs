using System;

namespace KargoTakip.Application.Helpers
{
    public abstract class BasePagedRequest
    {
        public int PageIndex { get; set; } = PaginationHelper.DefaultPageIndex;
        public int PageSize { get; set; } = PaginationHelper.DefaultPageSize;

        public int EffectivePageIndex => PaginationHelper.NormalizePageIndex(PageIndex);

        public int EffectivePageSize => PaginationHelper.NormalizePageSize(PageSize);

        public int Skip => PaginationHelper.GetSkip(EffectivePageIndex, EffectivePageSize);
    }
}
