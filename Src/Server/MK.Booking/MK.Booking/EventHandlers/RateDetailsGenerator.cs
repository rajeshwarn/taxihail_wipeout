using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class RateDetailsGenerator: IEventHandler<RateCreated>
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
                    FlatRate = @event.FlatRate,
                    DistanceMultiplicator = @event.DistanceMultiplicator,
                    TimeAdjustmentFactor = @event.TimeAdjustmentFactor,
                    PricePerPassenger = @event.PricePerPassenger,
                    DaysOfTheWeek = (int)@event.DaysOfTheWeek,
                });   
            }
        }
    }
}
