using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class RateDetailsGenerator: IEventHandler<RateCreated>, IEventHandler<RateUpdated>, IEventHandler<RateDeleted>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public RateDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(RateCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new RateDetail
                {
                    CompanyId = @event.SourceId,
                    Id = @event.RateId,
                    Name = @event.Name,
                    FlatRate = @event.FlatRate,
                    DistanceMultiplicator = @event.DistanceMultiplicator,
                    TimeAdjustmentFactor = @event.TimeAdjustmentFactor,
                    PricePerPassenger = @event.PricePerPassenger,
                    DaysOfTheWeek = (int)@event.DaysOfTheWeek,
                    StartTime =  @event.StartTime < (DateTime)SqlDateTime.MinValue ? (DateTime)SqlDateTime.MinValue:  @event.StartTime,
                    EndTime = @event.EndTime < (DateTime)SqlDateTime.MinValue ? (DateTime)SqlDateTime.MinValue : @event.EndTime,
                    Type = (int)@event.Type
                });   
            }
        }

        public void Handle(RateUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var rate = context.Find<RateDetail>(@event.RateId);
                rate.Name = @event.Name;
                rate.FlatRate = @event.FlatRate;
                rate.DistanceMultiplicator = @event.DistanceMultiplicator;
                rate.TimeAdjustmentFactor = @event.TimeAdjustmentFactor;
                rate.PricePerPassenger = @event.PricePerPassenger;
                rate.DaysOfTheWeek = (int) @event.DaysOfTheWeek;
                rate.StartTime = @event.StartTime < (DateTime)SqlDateTime.MinValue
                                ? (DateTime) SqlDateTime.MinValue
                                : @event.StartTime;
                rate.EndTime = @event.EndTime < (DateTime)SqlDateTime.MinValue
                              ? (DateTime) SqlDateTime.MinValue
                              : @event.EndTime;

                context.SaveChanges();
            }
            
        }

        public void Handle(RateDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var rate = context.Find<RateDetail>(@event.RateId);
                if (rate != null)
                {
                    context.Set<RateDetail>().Remove(rate);
                    context.SaveChanges();
                }
            }
        }
    }
}
