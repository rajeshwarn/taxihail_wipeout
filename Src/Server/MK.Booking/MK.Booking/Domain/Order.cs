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
        }

        public Order(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public Order(Guid id, Guid accountId, DateTime pickupDate, DateTime requestedDateTime, string friendlyName
            , string fullAddress, double longitude, double latitude, string apartment, string ringCode)
            : this(id)
        {
            if (Params.Get(friendlyName, fullAddress, longitude.ToString(CultureInfo.InvariantCulture), latitude.ToString(CultureInfo.InvariantCulture), apartment, ringCode).Any(p => p.IsNullOrEmpty())
                || pickupDate == null || requestedDateTime == null || accountId == null)
            {
                throw new InvalidOperationException("Missing required fields");
            }
            this.Update(new OrderCreated
            {
                SourceId = id,
                AccountId = accountId,
                PickupDate = pickupDate,
                RequestedDateTime = requestedDateTime,
                FriendlyName = friendlyName,
                FullAddress = fullAddress,
                Longitude = longitude,
                Latitude = latitude,
                Apartment = apartment,
                RingCode = ringCode
            });
        }

        private void OnOrderCreated(OrderCreated obj)
        {
            
        }
    }
}
