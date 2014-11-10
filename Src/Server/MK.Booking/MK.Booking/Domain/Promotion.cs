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

        private Dictionary<Guid, int> _usagesPerUser = new Dictionary<Guid, int>();
        private int _usages;

        public Promotion(Guid id) : base(id)
        {
            Handles<PromotionCreated>(OnPromotionCreated);
            Handles<PromotionUpdated>(OnPromotionUpdated);
            Handles<PromotionActivated>(OnPromotionActivated);
            Handles<PromotionDeactivated>(OnPromotionDeactivated);
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

        private void OnPromotionCreated(PromotionCreated @event)
        {
            _active = true;

            _startDate = @event.StartDate;
            _endDate = @event.EndDate;
            _startTime = @event.StartTime;
            _endTime = @event.EndTime;
            _daysOfWeek = @event.DaysOfWeek;
            _appliesToCurrentBooking = @event.AppliesToCurrentBooking;
            _appliesToFutureBooking = @event.AppliesToFutureBooking;
            _maxUsagesPerUser = @event.MaxUsagePerUser;
            _maxUsages = @event.MaxUsage;
        }

        private void OnPromotionUpdated(PromotionUpdated @event)
        {
            _startDate = @event.StartDate;
            _endDate = @event.EndDate;
            _startTime = @event.StartTime;
            _endTime = @event.EndTime;
            _daysOfWeek = @event.DaysOfWeek;
            _appliesToCurrentBooking = @event.AppliesToCurrentBooking;
            _appliesToFutureBooking = @event.AppliesToFutureBooking;
            _maxUsagesPerUser = @event.MaxUsagePerUser;
            _maxUsages = @event.MaxUsage;
        }

        private void OnPromotionActivated(PromotionActivated @event)
        {
            _active = true;
        }

        private void OnPromotionDeactivated(PromotionDeactivated @event)
        {
            _active = false;
        }
    }
}