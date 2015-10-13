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

        public Order(Guid id)
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
            Handles<OrderManuallyPairedForRideLinq>(NoAction);
            Handles<OrderUnpairedFromManualRideLinq>(NoAction);
            Handles<ManualRideLinqTripInfoUpdated>(NoAction);
            Handles<AutoTipUpdated>(NoAction);
            Handles<OriginalEtaLogged>(NoAction);
            Handles<OrderNotificationDetailUpdated>(NoAction);
			Handles<OrderReportCreated>(OnOrderReportCreated);
        }

        public Order(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

		public void UpdateOrderCreated(Guid accountId, DateTime pickupDate, Address pickupAddress, Address dropOffAddress, BookingSettings settings,
			double? estimatedFare, string userAgent, string clientLanguageCode, double? userLatitude, double? userLongitude, string userNote, string clientVersion,
			bool isChargeAccountPaymentWithCardOnFile, string companyKey, string companyName, string market, bool isPrepaid, decimal bookingFees, double? tipIncentive)
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
				IsPrepaid = isPrepaid,
				BookingFees = bookingFees,
                TipIncentive = tipIncentive
			});
		}

		public void UpdateOrderReportCreated(Guid accountId, DateTime pickupDate, Address pickupAddress, Address dropOffAddress, BookingSettings settings,
			double? estimatedFare, string userAgent, string clientLanguageCode, double? userLatitude, double? userLongitude, string userNote, string clientVersion,
			bool isChargeAccountPaymentWithCardOnFile, string companyKey, string companyName, string market, bool isPrepaid, decimal bookingFees, string error, double? tipIncentive)
		{
			Update(new OrderReportCreated
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
				IsPrepaid = isPrepaid,
				BookingFees = bookingFees,
				Error = error,
                TipIncentive = tipIncentive
			});
		}

        public void AddIbsOrderInfo(int ibsOrderId)
        {
            Update(new IbsOrderInfoAddedToOrder
            {
                IBSOrderId = ibsOrderId
            });
        }

		public void UpdateOrderManuallyPairedForRideLinq(Guid accountId, DateTime pairingDate, string pairingCode, string pairingToken, Address pickupAddress,
			string userAgent, string clientLanguageCode, string clientVersion, double? distance,
			double? total, double? fare, double? faireAtAlternateRate, double? tax, double? tip, double? toll,
			double? extra, double? surcharge, double? rateAtTripStart, double? rateAtTripEnd, string rateChangeTime, string medallion,
			string deviceName, int tripId, int driverId, double? accessFee, string lastFour)
		{
			Update(new OrderManuallyPairedForRideLinq
			{
				AccountId = accountId,
				PairingDate = pairingDate,
				UserAgent = userAgent,
				ClientLanguageCode = clientLanguageCode,
				ClientVersion = clientVersion,
				PairingCode = pairingCode,
				PairingToken = pairingToken,
				PickupAddress = pickupAddress,
				Total = total,
				Fare = fare,
				FareAtAlternateRate = faireAtAlternateRate,
				Tax = tax,
				Tip = tip,
				Toll = toll,
				Surcharge = surcharge,
				Extra = extra,
				RateAtTripStart = rateAtTripStart,
				RateAtTripEnd = rateAtTripEnd,
				RateChangeTime = rateChangeTime,
				Distance = distance,
				Medallion = medallion,
				DeviceName = deviceName,
				TripId = tripId,
				DriverId = driverId,
				AccessFee = accessFee,
				LastFour = lastFour
			});
		}

        public void UpdateRideLinqTripInfo(double? distance,double? total, double? fare, double? faireAtAlternateRate, double? tax, double? tip, double? toll,
            double? extra, double? surcharge, double? rateAtTripStart, double? rateAtTripEnd, string rateChangeTime, DateTime? startTime,
            DateTime? endTime, string pairingToken, int tripId, int driverId, double? accessFee, string lastFour, TollDetail[] tolls, double? lat, double? lon, string pairingError)
        {
            Update(new ManualRideLinqTripInfoUpdated
            {
                Distance = distance, 
                Total = total,
                Fare = fare,
                FareAtAlternateRate = faireAtAlternateRate,
                Tax = tax,
                Tip = tip,
                Toll = toll,
                Extra = extra,
                Surcharge = surcharge,
                RateAtTripStart = rateAtTripStart,
                RateAtTripEnd = rateAtTripEnd,
                RateChangeTime = rateChangeTime,
                StartTime = startTime,
                EndTime = endTime,
                PairingToken = pairingToken,
                TripId = tripId,
                DriverId = driverId,
                AccessFee = accessFee,
                LastFour = lastFour,
                Tolls = tolls,
                LastLatitudeOfVehicle = lat,
                LastLongitudeOfVehicle = lon,
                PairingError = pairingError
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

        public void UpdatePrepaidOrderPaymentInfo(Guid orderId, decimal totalAmount, decimal meterAmount, decimal taxAmount,
                decimal tipAmount, string transactionId, PaymentProvider provider, PaymentType type)
        {
            Update(new PrepaidOrderPaymentInfoUpdated
            {
                OrderId = orderId,
                Amount = totalAmount,
                Meter = meterAmount,
                Tax = taxAmount,
                Tip = tipAmount,
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

        public void ChangeStatus(OrderStatusDetail status, double? fare, double? tip, double? toll, double? tax, double? surcharge)
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
                    Surcharge = surcharge,
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

        public void UnpairFromRideLinq()
        {
            Update(new OrderUnpairedFromManualRideLinq());
        }

        public void PrepareForNextDispatch(string dispatchCompanyName, string dispatchCompanyKey)
        {
            Update(new OrderPreparedForNextDispatch
            {
                DispatchCompanyName = dispatchCompanyName,
                DispatchCompanyKey = dispatchCompanyKey
            });
        }

        public void SwitchOrderToNextDispatchCompany(int ibsOrderId, string companyKey, string companyName, string market, bool hasChangedBackToPaymentInCar)
        {
            Update(new OrderSwitchedToNextDispatchCompany
            {
                IBSOrderId = ibsOrderId,
                CompanyKey = companyKey,
                CompanyName = companyName,
                Market = market,
                HasChangedBackToPaymentInCar = hasChangedBackToPaymentInCar
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

        public void UpdateAutoTip(int autoTipPercentage)
        {
            Update(new AutoTipUpdated
            {
                AutoTipPercentage = autoTipPercentage
            });
        }

        public void LogOriginalEta(long originalEta)
        {
            Update(new OriginalEtaLogged
            {
                OriginalEta = originalEta
            });
        }

        public void UpdateOrderNotificationDetail(UpdateOrderNotificationDetail orderNotificationDetail)
        {
            Update(new OrderNotificationDetailUpdated
            {
                OrderId = orderNotificationDetail.OrderId,
                IsTaxiNearbyNotificationSent = orderNotificationDetail.IsTaxiNearbyNotificationSent,
                IsUnpairingReminderNotificationSent = orderNotificationDetail.IsUnpairingReminderNotificationSent,
                InfoAboutPaymentWasSentToDriver = orderNotificationDetail.InfoAboutPaymentWasSentToDriver
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

		public void OnOrderReportCreated(OrderReportCreated obj)
		{
			_status = OrderStatus.Unknown;
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