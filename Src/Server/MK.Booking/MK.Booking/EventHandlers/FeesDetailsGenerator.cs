using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class FeesDetailsGenerator :
        IEventHandler<FeesUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public FeesDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(FeesUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var existingFeesByMarket = context.Query<FeesDetail>().ToList();
                foreach (var newFees in @event.Fees)
                {
                    if (!newFees.Market.HasValue())
                    {
                        newFees.Market = null;
                    }

                    var feesToUpdate = existingFeesByMarket.SingleOrDefault(x => x.Market == newFees.Market) 
                                       ?? new FeesDetail { Id = Guid.NewGuid(), Market = newFees.Market };

                    feesToUpdate.Booking = newFees.Booking;
                    feesToUpdate.Cancellation = newFees.Cancellation;
                    feesToUpdate.NoShow = newFees.NoShow;

                    context.Save(feesToUpdate);
                }
            }
        }
    }
}