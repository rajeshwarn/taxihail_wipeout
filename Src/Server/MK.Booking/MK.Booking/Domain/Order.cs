#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
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
        private bool _isTimedOut;

        protected Order(Guid id)
            : base(id)
        {
            Handles<OrderCreated>(OnOrderCreated);
            Handles<OrderCancelled>(OnOrderCancelled);
            Handles<OrderRemovedFromHistory>(OnOrderRemoved);
            Handles<OrderRated>(OnOrderRated);
            Handles<PaymentInformationSet>(NoAction);
            Handles<OrderStatusChanged>(OnOrderStatusChanged);
            Handles<OrderPairedForPayment>(NoAction);
            Handles<OrderUnpairedForPayment>(NoAction);
            Handles<OrderTimedOut>(OnOrderTimedOut);
            Handles<OrderPreparedForNextDispatch>(NoAction);
            Handles<OrderSwitchedToNextDispatchCompany>(OnOrderSwitchedToNextDispatchCompany);
            Handles<DispatchCompanySwitchIgnored>(OnNextDispatchCompanySwitchIgnored);
            Handles<IbsOrderInfoAddedToOrder>(NoAction);
            Handles<OrderCancelledBecauseOfError>(NoAction);
            Handles<PrepaidOrderPaymentInfoUpdated>(NoAction);
            Handles<RefundedOrderUpdated>(NoAction);
            Handles<ManualRideLinqPaired>(NoAction);
            Handles<ManualRideLinqUnpaired>(NoAction);
            Handles<UpdatedManualRidelinqTripInfo>(NoAction);
        }


        public Order(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        /// <summary>
        /// Constructor for RideLinq
        /// </summary>
        public Order(Guid id, Guid accountId, DateTime startTime, string rideLinqId ,string userAgent, 
            string clientLanguageCode, string clientVersion, string companyKey, string companyName, string market) 
            : this(id)
        {
            Update(new ManualRideLinqPaired
            {
                AccountId = accountId,
                StartTime = startTime,
                UserAgent = userAgent,
                ClientLanguageCode = clientLanguageCode,
                ClientVersion = clientVersion,
                CompanyKey = companyKey,
                CompanyName = companyName,
                Market = market,
                RideLinqId = rideLinqId
            });
        }

        public Order(Guid id, Guid accountId, DateTime pickupDate, Address pickupAddress, Address dropOffAddress, BookingSettings settings,
            double? estimatedFare, string userAgent, string clientLanguageCode, double? userLatitude, double? userLongitude, string userNote, string clientVersion,
            bool isChargeAccountPaymentWithCardOnFile, string companyKey, string companyName, string market, bool isPrepaid)
            : this(id)
        {
            if ((settings == null) || pickupAddress == null || 
                (Params.Get(pickupAddress.FullAddress, settings.Name, settings.Phone).Any(p => p.IsNullOrEmpty())))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new OrderCreated
            {
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
                UserNote = userNote,
                ClientVersion = clientVersion,
                IsChargeAccountPaymentWithCardOnFile = isChargeAccountPaymentWithCardOnFile,
                CompanyKey = companyKey,
                CompanyName = companyName,
                Market = market,
                IsPrepaid = isPrepaid
            });
        }

        public void AddIbsOrderInfo(int ibsOrderId)
        {
            Update(new IbsOrderInfoAddedToOrder
            {
                IBSOrderId = ibsOrderId
            });
        }

        public void UpdateTripInfo(double? distance, double? faire, double? tax, double? tip, double? toll, double? extra, DriverInfos driverInfos, DateTime? endTime)
        {
            Update(new UpdatedManualRidelinqTripInfo
            {
                OrderId = Id,
                Distance = distance, 
                DriverInfo = driverInfos,
                Faire = faire,
                Tax = tax,
                Tip = tip,
                Toll = toll,
                Extra = extra,
                EndTime = endTime
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

        public void UpdatePrepaidOrderPaymentInfo(Guid orderId, decimal amount, decimal meter, decimal tax,
                decimal tip, string transactionId, PaymentProvider provider, PaymentType type)
        {
            Update(new PrepaidOrderPaymentInfoUpdated
            {
                OrderId = orderId,
                Amount = amount,
                Meter = meter,
                Tax = tax,
                Tip = tip,
                TransactionId = transactionId,
                Provider = provider,
                Type = type
            });
        }

        public void Cancel()
        {
            Update(new OrderCancelled());
        }

        public void CancelBecauseOfError(string errorCode, string errorDescription, bool wasPrepaid)
        {
            Update(new OrderCancelledBecauseOfError
            {
                ErrorCode = errorCode,
                ErrorDescription = errorDescription
            });
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

        public void ChangeStatus(OrderStatusDetail status, double? fare, double? tip, double? toll, double? tax)
        {
            if (status == null) throw new InvalidOperationException();

            if (status.Status != _status || status.IBSStatusId != _ibsStatus || _fare != fare)
            {
                Update(new OrderStatusChanged
                {
                    Status = status,
                    Fare = fare,
                    Tip = tip,
                    Toll = toll,
                    Tax = tax,
                    IsCompleted = status.Status == OrderStatus.Completed
                });
            }
        }

        public void NotifyOrderTimedOut(string market)
        {
            if (!_isTimedOut)
            {
                Update(new OrderTimedOut
                {
                    Market = market
                });  
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

        public void UnpairRideLinq()
        {
            Update(new ManualRideLinqUnpaired());
        }

        public void PrepareForNextDispatch(string dispatchCompanyName, string dispatchCompanyKey)
        {
            Update(new OrderPreparedForNextDispatch
            {
                DispatchCompanyName = dispatchCompanyName,
                DispatchCompanyKey = dispatchCompanyKey
            });
        }

        public void SwitchOrderToNextDispatchCompany(int ibsOrderId, string companyKey, string companyName, string market)
        {
            Update(new OrderSwitchedToNextDispatchCompany
            {
                IBSOrderId = ibsOrderId,
                CompanyKey = companyKey,
                CompanyName = companyName,
                Market = market
            });
        }

        public void IgnoreDispatchCompanySwitch()
        {
            Update(new DispatchCompanySwitchIgnored());
        }

        public void RefundedOrderUpdated(bool isSuccessful, string message)
        {
            Update(new RefundedOrderUpdated
            {
                IsSuccessful = isSuccessful,
                Message = message
            });
        }

        private void OnOrderStatusChanged(OrderStatusChanged @event)
        {
            // special case for migration
            if (@event.IsCompleted)
            {
                _status = OrderStatus.Completed;
            }

            if (@event.Status != null) //possible with migration
            {
                _ibsStatus = @event.Status.IBSStatusId;
                _status = @event.Status.Status;
            }

            if (@event.Fare.HasValue && @event.Fare.Value > 0)
            {
                _fare = @event.Fare;
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

        private void OnOrderTimedOut(OrderTimedOut obj)
        {
            _isTimedOut = true;
        }

        private void OnOrderSwitchedToNextDispatchCompany(OrderSwitchedToNextDispatchCompany obj)
        {
            _isTimedOut = false;
        }

        private void OnNextDispatchCompanySwitchIgnored(DispatchCompanySwitchIgnored obj)
        {
            _isTimedOut = false;
        }
    }
}