using KargoTakip.Application.Features.Cargo.Queries.GetAll;
using KargoTakip.Application.Helpers;
using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using System;
using System.Linq.Expressions;

namespace KargoTakip.Application.Specifications.CargoSpecifications
{
    public class CargoFilterSpecification : BaseSpecification<Cargo>
    {
        private const string OrderByDate = "date";
        private const string OrderByTrackingNumber = "trackingnumber";
        private const string OrderByStatus = "status";

        public CargoFilterSpecification(CargoFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            if (!filterParams.IsCount)
            {
                AddInclude(c => c.Sender);
                AddInclude(c => c.Receiver);
                if (filterParams.IncludeMovements)
                {
                    AddInclude(c => c.Movements);
                    ApplySplitQuery();
                }
            }

            var orderBy = string.IsNullOrEmpty(filterParams.OrderBy) ? OrderByDate : filterParams.OrderBy.Trim();
            var isDesc = filterParams.IsDescending;

            if (string.Equals(orderBy, OrderByTrackingNumber, StringComparison.OrdinalIgnoreCase))
            {
                if (isDesc) { ApplyOrderByDescending(c => c.TrackingNumber); ApplyThenByDescending(c => c.Id); }
                else { ApplyOrderBy(c => c.TrackingNumber); ApplyThenBy(c => c.Id); }
            }
            else if (string.Equals(orderBy, OrderByStatus, StringComparison.OrdinalIgnoreCase))
            {
                if (isDesc) { ApplyOrderByDescending(c => c.Status); ApplyThenByDescending(c => c.CreatedDate); }
                else { ApplyOrderBy(c => c.Status); ApplyThenBy(c => c.CreatedDate); }
            }
            else
            {
                if (isDesc) { ApplyOrderByDescending(c => c.CreatedDate); ApplyThenByDescending(c => c.Id); }
                else { ApplyOrderBy(c => c.CreatedDate); ApplyThenBy(c => c.Id); }
            }

            if (filterParams.PageSize > 0)
            {
                ApplyPaging(filterParams.PageSize * (filterParams.PageIndex - 1), filterParams.PageSize);
            }
        }

        private static Expression<Func<Cargo, bool>> BuildCriteria(CargoFilterParams filterParams)
        {
            Expression<Func<Cargo, bool>> criteria = c => !c.IsDeleted;

            if (filterParams.Status.HasValue)
            {
                var status = filterParams.Status.Value;
                criteria = CombineExpressions(criteria, c => c.Status == status);
            }

            if (!string.IsNullOrEmpty(filterParams.SearchTerm))
            {
                var searchTerm = filterParams.SearchTerm.Trim();
                if (filterParams.SearchTermTrackingOnly)
                {
                    criteria = CombineExpressions(criteria, c =>
                        c.TrackingNumber != null &&
                        (c.TrackingNumber.StartsWith(searchTerm) || c.TrackingNumber.Contains(searchTerm)));
                }
                else
                {
                    criteria = CombineExpressions(criteria, c =>
                        (c.TrackingNumber != null && (c.TrackingNumber == searchTerm || c.TrackingNumber.StartsWith(searchTerm) || c.TrackingNumber.Contains(searchTerm))) ||
                        (c.Sender != null && c.Sender.FullName != null && c.Sender.FullName.StartsWith(searchTerm)) ||
                        (c.Receiver != null && ((c.Receiver.Name != null && c.Receiver.Name.StartsWith(searchTerm)) || (c.Receiver.Surname != null && c.Receiver.Surname.StartsWith(searchTerm)) || (c.Receiver.Name + " " + c.Receiver.Surname).StartsWith(searchTerm))) ||
                        (c.Receiver != null && c.Receiver.Email != null && c.Receiver.Email.StartsWith(searchTerm))
                    );
                }
            }

            if (!string.IsNullOrEmpty(filterParams.TrackingNumber))
            {
                var trackingNumber = filterParams.TrackingNumber.Trim();
                criteria = CombineExpressions(criteria, c => c.TrackingNumber != null && c.TrackingNumber.StartsWith(trackingNumber));
            }

            if (!string.IsNullOrEmpty(filterParams.SenderName))
            {
                var senderName = filterParams.SenderName.Trim();
                criteria = CombineExpressions(criteria, c =>
                    c.Sender != null && c.Sender.FullName != null && c.Sender.FullName.Contains(senderName));
            }

            if (!string.IsNullOrEmpty(filterParams.ReceiverName))
            {
                var receiverName = filterParams.ReceiverName.Trim();
                criteria = CombineExpressions(criteria, c =>
                    c.Receiver != null &&
                    (c.Receiver.Name + " " + c.Receiver.Surname).Contains(receiverName));
            }

            if (!string.IsNullOrEmpty(filterParams.ReceiverEmail))
            {
                var receiverEmail = filterParams.ReceiverEmail.Trim();
                criteria = CombineExpressions(criteria, c =>
                    c.Receiver != null && c.Receiver.Email != null && c.Receiver.Email.Contains(receiverEmail));
            }

            if (filterParams.StartDate.HasValue)
            {
                var startDate = filterParams.StartDate.Value;
                criteria = CombineExpressions(criteria, c => c.CreatedDate >= startDate);
            }

            if (filterParams.EndDate.HasValue)
            {
                var endDate = filterParams.EndDate.Value;
                criteria = CombineExpressions(criteria, c => c.CreatedDate <= endDate);
            }

            return criteria;
        }

        public static (CargoFilterParams FilterParams, int PageIndex, int PageSize, bool RunCountQuery, bool FetchOneMore) FromGetAllCargosQuery(GetAllCargosQuery request)
        {
            var pageIndex = request.EffectivePageIndex;
            var pageSize = request.EffectivePageSize;
            var normalizedOrderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? OrderByDate : request.OrderBy.Trim();
            var hasSearchTerm = !string.IsNullOrWhiteSpace(request.SearchTerm);
            var fetchOneMore = !request.IncludeTotalCount || hasSearchTerm;
            var requestedPageSize = fetchOneMore ? (pageSize + 1) : pageSize;

            var (effectiveTrackingNumber, effectiveSearchTerm, searchTermTrackingOnly) = ResolveSearchAndTracking(
                request.SearchTerm,
                request.TrackingNumber);

            var filterParams = new CargoFilterParams
            {
                Status = request.GetCargoStatus(),
                TrackingNumber = effectiveTrackingNumber,
                SearchTerm = effectiveSearchTerm,
                SearchTermTrackingOnly = searchTermTrackingOnly,
                SenderName = request.SenderName,
                ReceiverName = request.ReceiverName,
                ReceiverEmail = request.ReceiverEmail,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                OrderBy = normalizedOrderBy,
                IsDescending = request.IsDescending,
                PageIndex = pageIndex,
                PageSize = requestedPageSize,
                IncludeMovements = false
            };

            return (filterParams, pageIndex, pageSize, request.IncludeTotalCount, fetchOneMore);
        }

        private static (string? EffectiveTrackingNumber, string? EffectiveSearchTerm, bool SearchTermTrackingOnly) ResolveSearchAndTracking(string? searchTerm, string? trackingNumber)
        {
            var searchTermTrimmed = searchTerm?.Trim() ?? string.Empty;
            var hasSearchTerm = !string.IsNullOrWhiteSpace(searchTerm);
            var searchTermLooksLikeTracking = hasSearchTerm && searchTermTrimmed.Length <= 40 && searchTermTrimmed.Any(char.IsDigit)
                && searchTermTrimmed.All(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '.' || ch == ' ');
            var trackingNumberFromRequest = !string.IsNullOrWhiteSpace(trackingNumber) ? trackingNumber!.Trim() : null;
            var useSearchTermAsTracking = searchTermLooksLikeTracking && trackingNumberFromRequest == null;
            var effectiveTrackingNumber = trackingNumberFromRequest ?? (useSearchTermAsTracking ? searchTermTrimmed : null);
            var effectiveSearchTerm = (useSearchTermAsTracking || (effectiveTrackingNumber != null && hasSearchTerm && searchTermLooksLikeTracking)) ? null : searchTerm;
            var searchTermTrackingOnly = !useSearchTermAsTracking && searchTermLooksLikeTracking;
            return (effectiveTrackingNumber, effectiveSearchTerm, searchTermTrackingOnly);
        }

        private static Expression<Func<T, bool>> CombineExpressions<T>(
            Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
            var left = leftVisitor.Visit(first.Body);

            var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
            var right = rightVisitor.Visit(second.Body);

            if (left == null || right == null)
                throw new InvalidOperationException("Expression visit produced null body.");

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }
    }

    file class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }

    public class CargoFilterParams
    {
        public CargoStatus? Status { get; set; }
        public string? TrackingNumber { get; set; }
        public string? SearchTerm { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverEmail { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OrderBy { get; set; } = "date";
        public bool IsDescending { get; set; } = true;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool IncludeMovements { get; set; } = false;
        public bool IsCount { get; set; } = false;
        public bool SearchTermTrackingOnly { get; set; } = false;

        /// <summary>Count için sadece filtre alanları kopyalanır; sıralama ve sayfalama bilgisi gerekmez.</summary>
        public CargoFilterParams ToCountParams()
        {
            return new CargoFilterParams
            {
                Status = Status,
                TrackingNumber = TrackingNumber,
                SearchTerm = SearchTerm,
                SearchTermTrackingOnly = SearchTermTrackingOnly,
                SenderName = SenderName,
                ReceiverName = ReceiverName,
                ReceiverEmail = ReceiverEmail,
                StartDate = StartDate,
                EndDate = EndDate,
                PageSize = 0,
                IncludeMovements = false,
                IsCount = true
            };
        }
    }
}
