using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Domain
{
    public class Promotion : EventSourced
    {
        private bool _active;

        private DateTime? _startDate;
        private DateTime? _endDate;
        private DateTime? _startTime;
        private DateTime? _endTime;
        private DayOfWeek[] _daysOfWeek;
        private bool _appliesToCurrentBooking;
        private bool _appliesToFutureBooking;
        private int? _maxUsagesPerUser;
        private int? _maxUsages;
        private string _code;
        private double _discountValue;
        private PromoDiscountType _discountType;

        private readonly Dictionary<Guid, int> _usagesPerUser = new Dictionary<Guid, int>();
        private int _usages;
        
        public Promotion(Guid id) : base(id)
        {
            Handles<PromotionCreated>(OnPromotionCreated);
            Handles<PromotionUpdated>(OnPromotionUpdated);
            Handles<PromotionActivated>(OnPromotionActivated);
            Handles<PromotionDeactivated>(OnPromotionDeactivated);
            Handles<PromotionUsed>(OnPromotionUsed);
        }

        public Promotion(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public Promotion(Guid id, string name, DateTime? startDate, DateTime? endDate, DateTime? startTime, 
            DateTime? endTime, DayOfWeek[] daysOfWeek, bool appliesToCurrentBooking, bool appliesToFutureBooking, 
            double discountValue,  PromoDiscountType discountType, int? maxUsagePerUser, int? maxUsage, string code)
            : this(id)
        {
            if (Params.Get(name, code).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new PromotionCreated
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                StartTime = startTime,
                EndTime = endTime,
                DaysOfWeek = daysOfWeek,
                AppliesToCurrentBooking = appliesToCurrentBooking,
                AppliesToFutureBooking = appliesToFutureBooking,
                DiscountValue = discountValue,
                DiscountType = discountType,
                MaxUsagePerUser = maxUsagePerUser,
                MaxUsage = maxUsage,
                Code = code
            });
        }

        public void Update(string name, DateTime? startDate, DateTime? endDate, DateTime? startTime, DateTime? endTime, 
            DayOfWeek[] daysOfWeek, bool appliesToCurrentBooking, bool appliesToFutureBooking, double discountValue, 
            PromoDiscountType discountType, int? maxUsagePerUser, int? maxUsage, string code)
        {
            if (Params.Get(name, code).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new PromotionUpdated
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                StartTime = startTime,
                EndTime = endTime,
                DaysOfWeek = daysOfWeek,
                AppliesToCurrentBooking = appliesToCurrentBooking,
                AppliesToFutureBooking = appliesToFutureBooking,
                DiscountValue = discountValue,
                DiscountType = discountType,
                MaxUsagePerUser = maxUsagePerUser,
                MaxUsage = maxUsage,
                Code = code
            });
        }

        public void Activate()
        {
            Update(new PromotionActivated());
        }

        public void Deactivate()
        {
            Update(new PromotionDeactivated());
        }

        public bool CanUse(Guid accountId, DateTime pickupDate, bool isFutureBooking, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!_active)
            {
                errorMessage = "This promotion is not active";
                return false;
            }

            if (_maxUsages.HasValue && _usages >= _maxUsages)
            {
                errorMessage = "Maximum number of uses has been reached for this promotion";
                return false;
            }

            if (_maxUsagesPerUser.HasValue)
            {
                var usagesForThisUser = _usagesPerUser.ContainsKey(accountId)
                    ? _usagesPerUser[accountId]
                    : 0;
                if (usagesForThisUser >= _maxUsagesPerUser)
                {
                    errorMessage = "Maximum number of uses has been reached for this promotion";
                    return false;
                }
            }

            if (isFutureBooking)
            {
                if (!_appliesToFutureBooking)
                {
                    errorMessage = "This promotion applies to current booking only";
                    return false;
                }
            }
            else
            {
                if (!_appliesToCurrentBooking)
                {
                    errorMessage = "This promotion applies to future booking only";
                    return false;
                }
            }

            if (!_daysOfWeek.Contains(pickupDate.DayOfWeek))
            {
                errorMessage = "This promotion is not available for this day of the week";
                return false;
            }

            if (_startDate.HasValue && _startDate > pickupDate)
            {
                errorMessage = "This promotion has not started yet";
                return false;
            }

            if (_endDate.HasValue && _endDate <= pickupDate)
            {
                errorMessage = "This promotion has expired";
                return false;
            }

            if (_startTime.HasValue)
            {
                var pickupTime = new DateTime(0, 0, 0, pickupDate.Hour, pickupDate.Minute, pickupDate.Second);
                if (_startTime.Value > pickupTime)
                {
                    errorMessage = "This promotion is not available at this time";
                    return false;
                }
            }

            if (_endTime.HasValue)
            {
                var pickupTime = new DateTime(0, 0, 0, pickupDate.Hour, pickupDate.Minute, pickupDate.Second);
                if (_endTime.Value <= pickupTime)
                {
                    errorMessage = "This promotion is not available at this time";
                    return false;
                }
            }

            return true;
        }

        public void Use(Guid orderId, Guid accountId, DateTime pickupDate, bool isFutureBooking)
        {
            string errorMessage;
            if (!CanUse(accountId, pickupDate, isFutureBooking, out errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            Update(new PromotionUsed
            {
                OrderId = orderId,
                AccountId = accountId,
                DiscountValue = _discountValue,
                DiscountType = _discountType,
                Code = _code
            });
        }

        private void OnPromotionCreated(PromotionCreated @event)
        {
            _active = true;

            _startDate = @event.StartDate;
            _endDate = @event.EndDate;
            _startTime = @event.StartTime.HasValue 
                ? new DateTime(0, 0, 0, @event.StartTime.Value.Hour, @event.StartTime.Value.Minute, @event.StartTime.Value.Second) 
                : (DateTime?) null;
            _endTime = @event.EndTime.HasValue
                ? new DateTime(0, 0, 0, @event.EndTime.Value.Hour, @event.EndTime.Value.Minute, @event.EndTime.Value.Second)
                : (DateTime?)null;
            _daysOfWeek = @event.DaysOfWeek;
            _appliesToCurrentBooking = @event.AppliesToCurrentBooking;
            _appliesToFutureBooking = @event.AppliesToFutureBooking;
            _maxUsagesPerUser = @event.MaxUsagePerUser;
            _maxUsages = @event.MaxUsage;
            _discountValue = @event.DiscountValue;
            _discountType = @event.DiscountType;
            _code = @event.Code;
        }

        private void OnPromotionUpdated(PromotionUpdated @event)
        {
            _startDate = @event.StartDate;
            _endDate = @event.EndDate;
            _startTime = @event.StartTime.HasValue
                ? new DateTime(0, 0, 0, @event.StartTime.Value.Hour, @event.StartTime.Value.Minute, @event.StartTime.Value.Second)
                : (DateTime?)null;
            _endTime = @event.EndTime.HasValue
                ? new DateTime(0, 0, 0, @event.EndTime.Value.Hour, @event.EndTime.Value.Minute, @event.EndTime.Value.Second)
                : (DateTime?)null;
            _daysOfWeek = @event.DaysOfWeek;
            _appliesToCurrentBooking = @event.AppliesToCurrentBooking;
            _appliesToFutureBooking = @event.AppliesToFutureBooking;
            _maxUsagesPerUser = @event.MaxUsagePerUser;
            _maxUsages = @event.MaxUsage;
            _discountValue = @event.DiscountValue;
            _discountType = @event.DiscountType;
            _code = @event.Code;
        }

        private void OnPromotionActivated(PromotionActivated @event)
        {
            _active = true;
        }

        private void OnPromotionDeactivated(PromotionDeactivated @event)
        {
            _active = false;
        }

        private void OnPromotionUsed(PromotionUsed @event)
        {
            _usages = _usages + 1;

            var usagesForThisUser = 0;
            if (_usagesPerUser.ContainsKey(@event.AccountId))
            {
                usagesForThisUser = _usagesPerUser[@event.AccountId];
            }

            _usagesPerUser[@event.AccountId] = usagesForThisUser + 1;
        }
    }
}