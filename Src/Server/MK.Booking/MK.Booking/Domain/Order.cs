using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Domain
{
    public class Order : EventSourced
    {
        protected Order(Guid id)
            : base(id)
        {
            base.Handles<OrderCreated>(OnOrderCreated);
            base.Handles<OrderCancelled>(OnOrderCancelled);
        }

        public Order(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public Order(Guid id, Guid accountId, DateTime pickupDate, string pickupAddress, double pickupLongitude,
                                                double pickupLatitude, string pickupAppartment, string pickupRingCode)
            : this(id)
        {
            //if (Params.Get(friendlyName, fullAddress, longitude.ToString(CultureInfo.InvariantCulture), latitude.ToString(CultureInfo.InvariantCulture), apartment, ringCode).Any(p => p.IsNullOrEmpty())
            //    || pickupDate == null || requestedDateTime == null || accountId == null)
            //{
            //    throw new InvalidOperationException("Missing required fields");
            //}
            this.Update(new OrderCreated
            {
                AccountId = accountId,
                PickupDate = pickupDate,
                PickupAddress = pickupAddress,
                PickupLongitude = pickupLongitude,
                PickupLatitude = pickupLatitude,
                PickupApartment = pickupAppartment,
                PickupRingCode = pickupRingCode,
                RequestedDate = DateTime.Now
            });
        }

        private void OnOrderCreated(OrderCreated obj)
        {
            
        }

        private void OnOrderCancelled(OrderCancelled obj)
        {
            
        }

        public void Cancel()
        {
            this.Update(new OrderCancelled());
        }
    }
}
