using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Domain
{
    public class Order : EventSourced
    {

        private OrderStatus _status;

        private bool _isRated;

        protected Order(Guid id)
            : base(id)
        {
            base.Handles<OrderCreated>(OnOrderCreated);
            base.Handles<OrderCancelled>(OnOrderCancelled);
            base.Handles<OrderCompleted>(OnOrderCompleted);
            base.Handles<OrderRemovedFromHistory>(OnOrderRemoved);
            base.Handles<OrderRated>(OnOrderRated);
            base.Handles<PaymentInformationSet>(OnPaymentInformationSet);
        }

        public Order(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public Order(Guid id, Guid accountId, int ibsOrderId, DateTime pickupDate, Address pickupAddress, Address dropOffAddress, BookingSettings settings): this(id)                   
        {
            if ((settings == null) || pickupAddress == null || ibsOrderId <= 0 ||
                 ( Params.Get(pickupAddress.FullAddress, settings.Name, settings.Phone).Any(p => p.IsNullOrEmpty()) ))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            this.Update(new OrderCreated
            {
                IBSOrderId = ibsOrderId,
                AccountId = accountId,
                PickupDate = pickupDate,
                PickupAddress = pickupAddress,
                DropOffAddress = dropOffAddress,
                CreatedDate = DateTime.Now,                
            });
        }

        public void SetPaymentInformation(PaymentInformation payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException("payment");
            }

            if (payment.CreditCardId == default(Guid))
            {
                throw new InvalidOperationException("CreditCardId not supplied");
            }
            if (payment.TipAmount < 0 || payment.TipPercent < 0)
            {
                throw new InvalidOperationException("Tip amount or tip percent must be greater than 0");
            }

            this.Update(new PaymentInformationSet
            {
                CreditCardId = payment.CreditCardId,
                TipAmount = payment.TipAmount,
                TipPercent = payment.TipPercent
            });
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

        public void RateOrder(string note, List<RatingScore> ratingScores)
        {
            if (!_isRated)
            {

                _isRated = true;
                Update(new OrderRated
                           {
                               Note = note,
                               RatingScores = ratingScores
                           });
            }
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

        private void OnOrderRated(OrderRated obj)
        {
            _isRated = true;
        }

        private void OnPaymentInformationSet(PaymentInformationSet @event)
        {

        }
    }
}
