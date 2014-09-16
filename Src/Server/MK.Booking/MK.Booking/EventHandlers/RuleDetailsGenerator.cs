#region

using System;
using System.Data.SqlTypes;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class RuleDetailsGenerator : IEventHandler<RuleCreated>, IEventHandler<RuleUpdated>,
        IEventHandler<RuleDeleted>, IEventHandler<RuleActivated>, IEventHandler<RuleDeactivated>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public RuleDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(RuleActivated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var rule = context.Find<RuleDetail>(@event.RuleId);
                rule.IsActive = true;
                context.SaveChanges();
            }
        }

        public void Handle(RuleCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new RuleDetail
                {
                    CompanyId = @event.SourceId,
                    Id = @event.RuleId,
                    Name = @event.Name,
                    Message = @event.Message,
                    ZoneRequired = @event.ZoneRequired,
                    ZoneList = @event.ZoneList,
                    DaysOfTheWeek = (int) @event.DaysOfTheWeek,
                    StartTime =
                        @event.StartTime.HasValue
                            ? @event.StartTime < (DateTime) SqlDateTime.MinValue
                                ? (DateTime) SqlDateTime.MinValue
                                : @event.StartTime
                            : null,
                    EndTime =
                        @event.EndTime.HasValue
                            ? @event.EndTime < (DateTime) SqlDateTime.MinValue
                                ? (DateTime) SqlDateTime.MinValue
                                : @event.EndTime
                            : null,
                    ActiveFrom =
                        @event.ActiveFrom.HasValue
                            ? @event.ActiveFrom < (DateTime) SqlDateTime.MinValue
                                ? (DateTime) SqlDateTime.MinValue
                                : @event.ActiveFrom
                            : null,
                    ActiveTo =
                        @event.ActiveTo.HasValue
                            ? @event.ActiveTo < (DateTime) SqlDateTime.MinValue
                                ? (DateTime) SqlDateTime.MinValue
                                : @event.ActiveTo
                            : null,
                    Priority = @event.Priority,
                    AppliesToCurrentBooking = @event.AppliesToCurrentBooking,
                    AppliesToFutureBooking = @event.AppliesToFutureBooking,
                    AppliesToPickup = @event.AppliesToPickup,
                    AppliesToDropoff = @event.AppliesToDropoff,
                    IsActive = @event.IsActive,
                    Category = (int) @event.Category,
                    Type = (int) @event.Type
                });
            }
        }

        public void Handle(RuleDeactivated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var rule = context.Find<RuleDetail>(@event.RuleId);
                rule.IsActive = false;
                context.SaveChanges();
            }
        }

        public void Handle(RuleDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var rule = context.Find<RuleDetail>(@event.RuleId);
                if (rule != null)
                {
                    context.Set<RuleDetail>().Remove(rule);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(RuleUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var rule = context.Find<RuleDetail>(@event.RuleId);
                rule.Name = @event.Name;
                rule.Message = @event.Message;
                rule.ZoneRequired = @event.ZoneRequired;
                rule.ZoneList = @event.ZoneList;
                rule.DaysOfTheWeek = (int) @event.DaysOfTheWeek;
                rule.StartTime = @event.StartTime.HasValue
                    ? @event.StartTime < (DateTime) SqlDateTime.MinValue
                        ? (DateTime) SqlDateTime.MinValue
                        : @event.StartTime
                    : null;
                rule.EndTime = @event.EndTime.HasValue
                    ? @event.EndTime < (DateTime) SqlDateTime.MinValue
                        ? (DateTime) SqlDateTime.MinValue
                        : @event.EndTime
                    : null;
                rule.ActiveFrom = @event.ActiveFrom.HasValue
                    ? @event.ActiveFrom < (DateTime) SqlDateTime.MinValue
                        ? (DateTime) SqlDateTime.MinValue
                        : @event.ActiveFrom
                    : null;
                rule.ActiveTo = @event.ActiveTo.HasValue
                    ? @event.ActiveTo < (DateTime) SqlDateTime.MinValue
                        ? (DateTime) SqlDateTime.MinValue
                        : @event.ActiveTo
                    : null;
                rule.Priority = @event.Priority;
                rule.AppliesToCurrentBooking = @event.AppliesToCurrentBooking;
                rule.AppliesToFutureBooking = @event.AppliesToFutureBooking;
                rule.AppliesToPickup = @event.AppliesToPickup;
                rule.AppliesToDropoff = @event.AppliesToDropoff;
                rule.IsActive = @event.IsActive;
                context.SaveChanges();
            }
        }
    }
}