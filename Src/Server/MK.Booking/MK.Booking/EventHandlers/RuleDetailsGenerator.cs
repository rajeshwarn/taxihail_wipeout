using System;
using System.Data.SqlTypes;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class RuleDetailsGenerator : 
        IEventHandler<RuleCreated>, 
        IEventHandler<RuleUpdated>,
        IEventHandler<RuleDeleted>, 
        IEventHandler<RuleActivated>, 
        IEventHandler<RuleDeactivated>
    {
        private readonly IProjectionSet<RuleDetail> _ruleProjectionSet;

        public RuleDetailsGenerator(IProjectionSet<RuleDetail> ruleProjectionSet)
        {
            _ruleProjectionSet = ruleProjectionSet;
        }
        
        public void Handle(RuleCreated @event)
        {
            _ruleProjectionSet.Add(new RuleDetail
            {
                CompanyId = @event.SourceId,
                Id = @event.RuleId,
                Name = @event.Name,
                Message = @event.Message,
                ZoneRequired = @event.ZoneRequired,
                ZoneList = @event.ZoneList,
                DaysOfTheWeek = (int)@event.DaysOfTheWeek,
                StartTime = SqlNullableDateTimeMinValueSafeGuard(@event.StartTime),
                EndTime = SqlNullableDateTimeMinValueSafeGuard(@event.EndTime),
                ActiveFrom = SqlNullableDateTimeMinValueSafeGuard(@event.ActiveFrom),
                ActiveTo = SqlNullableDateTimeMinValueSafeGuard(@event.ActiveTo),
                Priority = @event.Priority,
                AppliesToCurrentBooking = @event.AppliesToCurrentBooking,
                AppliesToFutureBooking = @event.AppliesToFutureBooking,
                AppliesToPickup = @event.AppliesToPickup,
                AppliesToDropoff = @event.AppliesToDropoff,
                IsActive = @event.IsActive,
                Category = (int)@event.Category,
                Type = (int)@event.Type,
                Market = @event.Market,
                DisableFutureBookingOnError = @event.DisableFutureBookingOnError
            });
        }

        public void Handle(RuleUpdated @event)
        {
            _ruleProjectionSet.Update(@event.RuleId, rule =>
            {
                rule.Name = @event.Name;
                rule.Message = @event.Message;
                rule.ZoneRequired = @event.ZoneRequired;
                rule.ExcludeCircularZone = @event.ExcludeCircularZone;
                rule.ExcludedCircularZoneLatitude = @event.ExcludedCircularZoneLatitude;
                rule.ExcludedCircularZoneLongitude = @event.ExcludedCircularZoneLongitude;
                rule.ExcludedCircularZoneRadius = @event.ExcludedCircularZoneRadius;
                rule.ZoneList = @event.ZoneList;
                rule.DaysOfTheWeek = (int)@event.DaysOfTheWeek;
                rule.StartTime = SqlNullableDateTimeMinValueSafeGuard(@event.StartTime);
                rule.EndTime = SqlNullableDateTimeMinValueSafeGuard(@event.EndTime);
                rule.ActiveFrom = SqlNullableDateTimeMinValueSafeGuard(@event.ActiveFrom);
                rule.ActiveTo = SqlNullableDateTimeMinValueSafeGuard(@event.ActiveTo);
                rule.Priority = @event.Priority;
                rule.AppliesToCurrentBooking = @event.AppliesToCurrentBooking;
                rule.AppliesToFutureBooking = @event.AppliesToFutureBooking;
                rule.AppliesToPickup = @event.AppliesToPickup;
                rule.AppliesToDropoff = @event.AppliesToDropoff;
                rule.IsActive = @event.IsActive;
                rule.Market = @event.Market;
                rule.DisableFutureBookingOnError = @event.DisableFutureBookingOnError;
            });
        }

        public void Handle(RuleDeleted @event)
        {
            _ruleProjectionSet.Remove(@event.RuleId);
        }

        public void Handle(RuleActivated @event)
        {
            _ruleProjectionSet.Update(@event.RuleId, rule =>
            {
                rule.IsActive = true;
            });
        }

        public void Handle(RuleDeactivated @event)
        {
            _ruleProjectionSet.Update(@event.RuleId, rule =>
            {
                rule.IsActive = false;
            });
        }
        
        private DateTime? SqlNullableDateTimeMinValueSafeGuard(DateTime? value)
        {
            return value.HasValue
                ? value < (DateTime)SqlDateTime.MinValue
                    ? (DateTime)SqlDateTime.MinValue
                    : value
                : null;
        }
    }
}