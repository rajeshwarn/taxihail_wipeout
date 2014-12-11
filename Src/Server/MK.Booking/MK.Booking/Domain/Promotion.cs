using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Domain
{
    public class Promotion : EventSourced
    {
        private bool _active;
        private PromotionTriggerSettings _triggerSettings;

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
        private decimal _discountValue;
        private PromoDiscountType _discountType;

        private readonly Dictionary<Guid, int> _usagesPerUser = new Dictionary<Guid, int>();
        private readonly List<Guid> _orderIds = new List<Guid>(); 
        private int _usages;
        
        public Promotion(Guid id) : base(id)
        {
            Handles<PromotionCreated>(OnPromotionCreated);
            Handles<PromotionUpdated>(OnPromotionUpdated);
            Handles<PromotionActivated>(OnPromotionActivated);
            Handles<PromotionDeactivated>(OnPromotionDeactivated);
            Handles<PromotionApplied>(OnPromotionApplied);
            Handles<PromotionRedeemed>(NoAction);
        }

        public Promotion(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public Promotion(Guid id, string name, string description, DateTime? startDate, DateTime? endDate, DateTime? startTime, 
            DateTime? endTime, DayOfWeek[] daysOfWeek, bool appliesToCurrentBooking, bool appliesToFutureBooking,
            decimal discountValue, PromoDiscountType discountType, int? maxUsagePerUser, int? maxUsage, string code,
            DateTime? publishedStartDate, DateTime? publishedEndDate, PromotionTriggerSettings triggerSettings)
            : this(id)
        {
            if (Params.Get(name, code).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new PromotionCreated
            {
                Name = name,
                Description = description,
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
                Code = code,
                PublishedStartDate = publishedStartDate,
                PublishedEndDate = publishedEndDate,
                TriggerSettings = triggerSettings
            });
        }

        public void Update(string name, string description, DateTime? startDate, DateTime? endDate, DateTime? startTime, DateTime? endTime,
            DayOfWeek[] daysOfWeek, bool appliesToCurrentBooking, bool appliesToFutureBooking, decimal discountValue, PromoDiscountType discountType,
            int? maxUsagePerUser, int? maxUsage, string code, DateTime? publishedStartDate, DateTime? publishedEndDate, PromotionTriggerSettings triggerSettings)
        {
            if (Params.Get(name, code).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new PromotionUpdated
            {
                Name = name,
                Description = description,
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
                Code = code,
                PublishedStartDate = publishedStartDate,
                PublishedEndDate = publishedEndDate,
                TriggerSettings = triggerSettings
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

        public bool CanApply(Guid accountId, DateTime pickupDate, bool isFutureBooking, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!_active)
            {
                errorMessage = "CannotCreateOrder_PromotionIsNotActive";
                return false;
            }

            if (_maxUsages.HasValue && _usages >= _maxUsages)
            {
                errorMessage = "CannotCreateOrder_PromotionHasReachedMaxUsage";
                return false;
            }

            if (_maxUsagesPerUser.HasValue)
            {
                int usagesForThisUser;
                _usagesPerUser.TryGetValue(accountId, out usagesForThisUser);
                if (usagesForThisUser >= _maxUsagesPerUser)
                {
                    errorMessage = "CannotCreateOrder_PromotionHasReachedMaxUsage";
                    return false;
                }
            }

            if (isFutureBooking)
            {
                if (!_appliesToFutureBooking)
                {
                    errorMessage = "CannotCreateOrder_PromotionAppliesToCurrentBookingOnly";
                    return false;
                }
            }
            else
            {
                if (!_appliesToCurrentBooking)
                {
                    errorMessage = "CannotCreateOrder_PromotionAppliesToFutureBookingOnly";
                    return false;
                }
            }

            if (!_daysOfWeek.Contains(pickupDate.DayOfWeek))
            {
                errorMessage = "CannotCreateOrder_PromotionNotAvailableForThisDayOfTheWeek";
                return false;
            }

            if (_startDate.HasValue && _startDate > pickupDate)
            {
                errorMessage = "CannotCreateOrder_PromotionNotStartedYet";
                return false;
            }

            if (_endDate.HasValue && _endDate <= pickupDate)
            {
                errorMessage = "CannotCreateOrder_PromotionHasExpired";
                return false;
            }

            if (_startTime.HasValue && _endTime.HasValue)
            {
                var pickupTime = DateTime.MinValue.AddHours(pickupDate.Hour).AddMinutes(pickupDate.Minute);
                var isInRange = pickupTime >= _startTime.Value && pickupTime < _endTime.Value;
                if (!isInRange)
                {
                    errorMessage = "CannotCreateOrder_PromotionNotAvailableAtThisTime";
                    return false;
                }
            }

            return true;
        }

        public void Apply(Guid orderId, Guid accountId, DateTime pickupDate, bool isFutureBooking)
        {
            string errorMessage;
            if (!CanApply(accountId, pickupDate, isFutureBooking, out errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            Update(new PromotionApplied
            {
                OrderId = orderId,
                AccountId = accountId,
                DiscountValue = _discountValue,
                DiscountType = _discountType,
                Code = _code
            });
        }

        public decimal GetAmountSaved(decimal totalAmountOfOrder)
        {
            if (_discountType == PromoDiscountType.Cash)
            {
                // return smallest value to make sure we don't credit user
                return Math.Min(totalAmountOfOrder, _discountValue);
            }

            if (_discountType == PromoDiscountType.Percentage)
            {
                var amountSaved = totalAmountOfOrder * (_discountValue / 100);
                return Math.Round(amountSaved, 2);
            }

            return 0;
        }

        public void Redeem(Guid orderId, decimal totalAmountOfOrder)
        {
            if (!_orderIds.Contains(orderId))
            {
                throw new InvalidOperationException("Promotion must be applied to an order before being redeemed");
            }

            var amountSaved = GetAmountSaved(totalAmountOfOrder);

            Update(new PromotionRedeemed
            {
                OrderId = orderId,
                AmountSaved = amountSaved
            });
        }

        private void OnPromotionCreated(PromotionCreated @event)
        {
            _startDate = @event.StartDate;
            _endDate = @event.EndDate;
            _daysOfWeek = @event.DaysOfWeek;
            _appliesToCurrentBooking = @event.AppliesToCurrentBooking;
            _appliesToFutureBooking = @event.AppliesToFutureBooking;
            _maxUsagesPerUser = @event.MaxUsagePerUser;
            _maxUsages = @event.MaxUsage;
            _discountValue = @event.DiscountValue;
            _discountType = @event.DiscountType;
            _code = @event.Code;
            _triggerSettings = @event.TriggerSettings;

            SetInternalStartAndEndTimes(@event.StartTime, @event.EndTime);
        }

        private void OnPromotionUpdated(PromotionUpdated @event)
        {
            _startDate = @event.StartDate;
            _endDate = @event.EndDate;
            _daysOfWeek = @event.DaysOfWeek;
            _appliesToCurrentBooking = @event.AppliesToCurrentBooking;
            _appliesToFutureBooking = @event.AppliesToFutureBooking;
            _maxUsagesPerUser = @event.MaxUsagePerUser;
            _maxUsages = @event.MaxUsage;
            _discountValue = @event.DiscountValue;
            _discountType = @event.DiscountType;
            _code = @event.Code;
            _triggerSettings = @event.TriggerSettings;

            SetInternalStartAndEndTimes(@event.StartTime, @event.EndTime);
        }

        private void OnPromotionActivated(PromotionActivated @event)
        {
            _active = true;
        }

        private void OnPromotionDeactivated(PromotionDeactivated @event)
        {
            _active = false;
        }

        private void OnPromotionApplied(PromotionApplied @event)
        {
            _usages = _usages + 1;

            int usagesForThisUser;
            _usagesPerUser.TryGetValue(@event.AccountId, out usagesForThisUser);
            _usagesPerUser[@event.AccountId] = usagesForThisUser + 1;

            _orderIds.Add(@event.OrderId);
        }

        /// <summary>
        /// Sets the start/end times with DateTime.MinValue to be used when validating time of day for promo usage
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        private void SetInternalStartAndEndTimes(DateTime? startTime, DateTime? endTime)
        {
            if (startTime.HasValue && endTime.HasValue)
            {
                // The date received here are always the current day or tomorrow (verified with model validation)
                // if the date is different, we assume that it's the end time is on the next day
                var dayOffset = 0;
                if (startTime.Value.Date != endTime.Value.Date)
                {
                    // end time is on the next day
                    dayOffset = 1;
                }

                _startTime = DateTime.MinValue.AddHours(startTime.Value.Hour).AddMinutes(startTime.Value.Minute);
                _endTime = DateTime.MinValue.AddDays(dayOffset).AddHours(endTime.Value.Hour).AddMinutes(endTime.Value.Minute);
            }
            else
            {
                _startTime = null;
                _endTime = null;
            }
        }
    }
}