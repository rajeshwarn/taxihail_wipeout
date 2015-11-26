using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;
using RestSharp.Extensions;

namespace apcurium.MK.Booking.EventHandlers
{
    public class FeesDetailsGenerator :
        IEventHandler<FeesUpdated>
    {
        private readonly IProjectionSet<FeesDetail> _feesProjectionSet;

        public FeesDetailsGenerator(IProjectionSet<FeesDetail> feesProjectionSet)
        {
            _feesProjectionSet = feesProjectionSet;
        }

        public void Handle(FeesUpdated @event)
        {
            foreach (var newFees in @event.Fees)
            {
                if (!newFees.Market.HasValue())
                {
                    newFees.Market = null;
                }

                if (!_feesProjectionSet.Exists(x => x.Market == newFees.Market))
                {
                    _feesProjectionSet.Add(new FeesDetail
                    {
                        Id = Guid.NewGuid(),
                        Market = newFees.Market,
                        Booking = newFees.Booking,
                        Cancellation = newFees.Cancellation,
                        NoShow = newFees.NoShow
                    });
                }
                else
                {
                    _feesProjectionSet.Update(x => x.Market == newFees.Market, feesToUpdate =>
                    {
                        feesToUpdate.Booking = newFees.Booking;
                        feesToUpdate.Cancellation = newFees.Cancellation;
                        feesToUpdate.NoShow = newFees.NoShow;
                    });
                }
            }
        }
    }
}