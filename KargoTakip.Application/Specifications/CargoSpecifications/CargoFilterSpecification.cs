using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using System;
using System.Linq.Expressions;

namespace KargoTakip.Application.Specifications.CargoSpecifications
{
    public class CargoFilterSpecification : BaseSpecification<Cargo>
    {
        public CargoFilterSpecification(CargoFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            if (!filterParams.IsCount && !filterParams.SkipIncludes)
            {
                AddInclude(c => c.Sender);
                AddInclude(c => c.Receiver);
                if (filterParams.IncludeMovements)
                {
                    AddInclude(c => c.Movements);
                    ApplySplitQuery();
                }
            }

            if (!string.IsNullOrEmpty(filterParams.OrderBy))
            {
                switch (filterParams.OrderBy.ToLower())
                {
                    case "date":
                        if (filterParams.IsDescending)
                        {
                            ApplyOrderByDescending(c => c.CreatedDate);
                            ApplyThenByDescending(c => c.Id);
                        }
                        else
                        {
                            ApplyOrderBy(c => c.CreatedDate);
                            ApplyThenBy(c => c.Id);
                        }
                        break;
                    case "trackingnumber":
                        if (filterParams.IsDescending)
                        {
                            ApplyOrderByDescending(c => c.TrackingNumber);
                            ApplyThenByDescending(c => c.Id);
                        }
                        else
                        {
                            ApplyOrderBy(c => c.TrackingNumber);
                            ApplyThenBy(c => c.Id);
                        }
                        break;
                    case "status":
                        if (filterParams.IsDescending)
                        {
                            ApplyOrderByDescending(c => c.Status);
                            ApplyThenByDescending(c => c.CreatedDate);
                        }
                        else
                        {
                            ApplyOrderBy(c => c.Status);
                            ApplyThenBy(c => c.CreatedDate);
                        }
                        break;
                    default:
                        ApplyOrderByDescending(c => c.CreatedDate);
                        ApplyThenByDescending(c => c.Id);
                        break;
                }
            }
            else
            {
                ApplyOrderByDescending(c => c.CreatedDate);
                ApplyThenByDescending(c => c.Id);
            }

            if (filterParams.PageSize > 0)
            {
                if (filterParams.UseKeysetPagination)
                {
                    ApplyPaging(0, filterParams.PageSize);
                }
                else
                {
                    ApplyPaging(filterParams.PageSize * (filterParams.PageIndex - 1), filterParams.PageSize);
                }
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
                var searchTermRaw = filterParams.SearchTerm.Trim();
                var searchTermLower = searchTermRaw.ToLower();
                criteria = CombineExpressions(criteria, c =>
                    c.TrackingNumber == searchTermRaw ||
                    c.TrackingNumber.StartsWith(searchTermRaw) ||
                    (c.Sender != null && c.Sender.FullName.ToLower().Contains(searchTermLower)) ||
                    (c.Receiver != null && (c.Receiver.Name + " " + c.Receiver.Surname).ToLower().Contains(searchTermLower)) ||
                    (c.Receiver != null && c.Receiver.Email != null && c.Receiver.Email.ToLower().Contains(searchTermLower))
                );
            }

            if (!string.IsNullOrEmpty(filterParams.TrackingNumber))
            {
                var trackingNumber = filterParams.TrackingNumber.Trim();
                criteria = CombineExpressions(criteria, c => c.TrackingNumber.StartsWith(trackingNumber));
            }

            if (!string.IsNullOrEmpty(filterParams.SenderName))
            {
                var senderName = filterParams.SenderName.ToLower();
                criteria = CombineExpressions(criteria, c =>
                    c.Sender != null && c.Sender.FullName.ToLower().Contains(senderName));
            }

            if (!string.IsNullOrEmpty(filterParams.ReceiverName))
            {
                var receiverName = filterParams.ReceiverName.ToLower();
                criteria = CombineExpressions(criteria, c =>
                    c.Receiver != null &&
                    (c.Receiver.Name + " " + c.Receiver.Surname).ToLower().Contains(receiverName));
            }

            if (!string.IsNullOrEmpty(filterParams.ReceiverEmail))
            {
                var receiverEmail = filterParams.ReceiverEmail.ToLower();
                criteria = CombineExpressions(criteria, c =>
                    c.Receiver != null &&
                    c.Receiver.Email.ToLower().Contains(receiverEmail));
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

            if (filterParams.UseKeysetPagination
                && filterParams.CursorCreatedDate.HasValue
                && !string.IsNullOrEmpty(filterParams.CursorId)
                && string.Equals(filterParams.OrderBy, "date", StringComparison.OrdinalIgnoreCase))
            {
                var cursorDate = filterParams.CursorCreatedDate.Value;
                var cursorId = filterParams.CursorId;

                if (filterParams.IsDescending)
                {
                    criteria = CombineExpressions(criteria, c =>
                        c.CreatedDate < cursorDate ||
                        (c.CreatedDate == cursorDate && string.Compare(c.Id, cursorId) < 0));
                }
                else
                {
                    criteria = CombineExpressions(criteria, c =>
                        c.CreatedDate > cursorDate ||
                        (c.CreatedDate == cursorDate && string.Compare(c.Id, cursorId) > 0));
                }
            }

            return criteria;
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

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left!, right!), parameter);
        }
    }

    public class ReplaceExpressionVisitor : ExpressionVisitor
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
        public bool UseKeysetPagination { get; set; } = false;
        public DateTime? CursorCreatedDate { get; set; }
        public string? CursorId { get; set; }
        public bool IsCount { get; set; } = false;
        public bool SkipIncludes { get; set; } = false;
    }
}
