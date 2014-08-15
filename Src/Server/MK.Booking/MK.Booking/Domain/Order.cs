#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Domain
{
    public class Order : EventSourced
    {
        private string _ibsStatus;
        private bool _isRated;
        private OrderStatus _status;
        private double? _fare;

        protected Order(Guid id)
            : base(id)
        {
            Handles<OrderCreated>(OnOrderCreated);
            Handles<OrderCancelled>(OnOrderCancelled);
            Handles<OrderRemovedFromHistory>(OnOrderRemoved);
            Handles<OrderRated>(OnOrderRated);
            Handles<PaymentInformationSet>(NoAction);
            Handles<OrderStatusChanged>(OnOrderStatusChanged);
            Handles<OrderFareUpdated>(OnOrderFareUpdated);
            Handles<OrderPairedForPayment>(NoAction);
            Handles<OrderUnpairedForPayment>(NoAction);
        }

        public Order(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public Order(Guid id, Guid accountId, int ibsOrderId, DateTime pickupDate, Address pickupAddress,
            Address dropOffAddress, BookingSettings settings, double? estimatedFare, string userAgent,
            string clientLanguageCode, double? userLatitude, double? userLongitude, string clientVersion) : this(id)
        {
            if ((settings == null) || pickupAddress == null || ibsOrderId <= 0 ||
                (Params.Get(pickupAddress.FullAddress, settings.Name, settings.Phone).Any(p => p.IsNullOrEmpty())))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new OrderCreated
            {
                IBSOrderId = ibsOrderId,
                AccountId = accountId,
                PickupDate = pickupDate,
                PickupAddress = pickupAddress,
                DropOffAddress = dropOffAddress,
                Settings = settings,
                EstimatedFare = estimatedFare,
                CreatedDate = DateTime.Now,
                UserAgent = userAgent,
                ClientLanguageCode = clientLanguageCode,
                UserLatitude = userLatitude,
                UserLongitude = userLongitude,
                ClientVersion = clientVersion
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

            Update(new PaymentInformationSet
            {
                CreditCardId = payment.CreditCardId,
                TipAmount = payment.TipAmount,
                TipPercent = payment.TipPercent
            });
        }


        public void Cancel()
        {
            Update(new OrderCancelled());
        }

        public void RemoveFromHistory()
        {
            Update(new OrderRemovedFromHistory());
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

        public void ChangeStatus(OrderStatusDetail status)
        {
            if (status == null) throw new InvalidOperationException();

            if (status.IBSStatusId != _ibsStatus)
            {
                Update(new OrderStatusChanged
                {
                    Status = status
                });
            }
        }

        public void AddFareInformation(double fare, double tip, double toll, double tax)
        {
            if (_fare != fare)
            {
                Update(new OrderFareUpdated{ Fare = fare, Tip = tip, Toll = toll, Tax = tax});
            }
        }

        public void Pair(string medallion, string driverId, string pairingToken, string pairingCode,
            string tokenOfCardToBeUsedForPayment, double? autoTipAmount, int? autoTipPercentage)
        {
            Update(new OrderPairedForPayment
            {
                Medallion = medallion,
                DriverId = driverId,
                PairingToken = pairingToken,
                PairingCode = pairingCode,
                TokenOfCardToBeUsedForPayment = tokenOfCardToBeUsedForPayment,
                AutoTipAmount = autoTipAmount,
                AutoTipPercentage = autoTipPercentage
            });
        }

        public void Unpair()
        {
            Update(new OrderUnpairedForPayment());
        }

        private void OnOrderStatusChanged(OrderStatusChanged @event)
        {
            _ibsStatus = @event.Status.IBSStatusId;

            if (@event.Status.Status == OrderStatus.Completed)
            {
                _status = OrderStatus.Completed;
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

        private void OnOrderRemoved(OrderRemovedFromHistory obj)
        {
            _status = OrderStatus.Removed;
        }

        private void OnOrderRated(OrderRated obj)
        {
            _isRated = true;
        }

        private void OnOrderFareUpdated(OrderFareUpdated @event)
        {
            _fare = @event.Fare;
        }
    }
}