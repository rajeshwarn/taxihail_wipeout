using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Domain
{
    public class Order : EventSourced
    {

        private OrderStatus _status;

        protected Order(Guid id)
            : base(id)
        {
            base.Handles<OrderCreated>(OnOrderCreated);
            base.Handles<OrderCancelled>(OnOrderCancelled);
            base.Handles<OrderCompleted>(OnOrderCompleted);
            base.Handles<OrderRemovedFromHistory>(OnOrderRemoved);
        }

        public Order(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public Order(Guid id, Guid accountId, int ibsOrderId, DateTime pickupDate, string pickupAddress, double pickupLongitude,
                                                double pickupLatitude, string pickupAppartment, string pickupRingCode,
                                                string dropOffAddress, double? dropOffLongitude, double? dropOffLatitude,
                                                BookingSettings settings): this(id)                   
        {
            if ((settings == null) || pickupLatitude == 0 || pickupLongitude == 0 || ibsOrderId <= 0 ||
                 ( Params.Get(pickupAddress, settings.Name, settings.Phone).Any(p => p.IsNullOrEmpty()) ))
            {
                throw new InvalidOperationException("Missing required fields");
            }
            this.Update(new OrderCreated
            {
                IBSOrderId = ibsOrderId,
                AccountId = accountId,
                PickupDate = pickupDate,
                PickupAddress = pickupAddress,
                PickupLongitude = pickupLongitude,
                PickupLatitude = pickupLatitude,
                PickupApartment = pickupAppartment,
                PickupRingCode = pickupRingCode,
                DropOffAddress = dropOffAddress,
                DropOffLongitude = dropOffLongitude,
                DropOffLatitude = dropOffLatitude,
                CreatedDate = DateTime.Now,                
            });
        }

        private void OnOrderCreated(OrderCreated obj)
        {
            _status = OrderStatus.Created;   
        }

        private void OnOrderCancelled(OrderCancelled obj)
        {
            _status = OrderStatus.Canceled;   
        }

        private void OnOrderCompleted(OrderCompleted obj)
        {
            _status = OrderStatus.Completed;
        }

        private void OnOrderRemoved(OrderRemovedFromHistory obj)
        {
            _status = OrderStatus.Removed;
        }

        public void Cancel()
        {
            this.Update(new OrderCancelled());
        }

        public void Complete(DateTime date, double? fare, double? tip, double? toll)
        {
            if(_status != OrderStatus.Completed)
            {
                
                this.Update(new OrderCompleted
                                {
                                    Date = date,
                                    Fare = fare,
                                    Toll = toll,
                                    Tip = tip
                                });
            }
        }

        public void RemoveFromHistory()
        {
            this.Update(new OrderRemovedFromHistory());
        }
    }
}
