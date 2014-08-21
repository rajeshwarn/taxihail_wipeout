#region

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using System;

#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class OrderStatusUpdater
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IOrderDao _orderDao;
        private readonly IPaymentService _paymentService;
        private readonly Resources.Resources _resources;

        public OrderStatusUpdater(IConfigurationManager configurationManager, 
            ICommandBus commandBus, 
            IOrderPaymentDao orderPaymentDao, 
            IOrderDao orderDao,
            IPaymentService paymentService)
        {
            _orderDao = orderDao;
            _paymentService = paymentService;
            _configurationManager = configurationManager;
            _commandBus = commandBus;
            _orderPaymentDao = orderPaymentDao;
            _resources = new Resources.Resources(configurationManager.GetSetting("TaxiHail.ApplicationKey"));
        }

        private void UpdateVehiclePositionIfNecessary(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatus)
        {
            //todo dao => change only position
            //todo call push notification from here trough a service
            //todo remove param out
            if (orderStatus.VehicleLatitude != ibsOrderInfo.VehicleLatitude
                || orderStatus.VehicleLongitude != ibsOrderInfo.VehicleLongitude)
            {
                bool taxiNearbyPushSent;
                _orderDao.UpdateVehiclePosition(orderStatus.OrderId, 
                                                    ibsOrderInfo.Status, 
                                                    ibsOrderInfo.VehicleLatitude, ibsOrderInfo.VehicleLongitude, out taxiNearbyPushSent);
                
                // modify orderStatus object here to make sure that if the status is changed and a command is sent
                // using this object, the values set in the dao will not be erased by automapping in the eventhandler
                orderStatus.VehicleLatitude = ibsOrderInfo.VehicleLatitude;
                orderStatus.VehicleLongitude = ibsOrderInfo.VehicleLongitude;
                orderStatus.IsTaxiNearbyNotificationSent = taxiNearbyPushSent;
            }
        }

        private void HandlePairingForRideLinqCmt(OrderPairingDetail pairingInfo, IBSOrderInformation ibsOrderInfo)
        {
            // in the case of RideLinq CMT, we only want to calculate the tip to fill information on our side
            if (pairingInfo.AutoTipPercentage.HasValue)
            {
                ibsOrderInfo.Tip = GetTipAmount(ibsOrderInfo.Fare, pairingInfo.AutoTipPercentage.Value);
            }
        }

        private void HandlePairingForStandardPairing(OrderStatusDetail orderStatusDetail, OrderPairingDetail pairingInfo, IBSOrderInformation ibsOrderInfo)
        {
            if (!_configurationManager.GetSetting("AutomaticPayment", false))
            {
                // Automatic payment is disabled, nothing to do here
                return;
            }

            var orderPayment = _orderPaymentDao.FindByOrderId(orderStatusDetail.OrderId);
            if (orderPayment != null && orderPayment.IsCompleted)
            {
                // Payment was completed
                return;
            }

            if (ibsOrderInfo.Fare <= 0)
            {
                // fare was not returned by ibs
                // check if status is completed
                if (orderStatusDetail.Status == OrderStatus.Completed)
                {
                    // no fare received but order is completed, change status to increase polling speed
                    orderStatusDetail.Status = OrderStatus.WaitingForPayment;
                    orderStatusDetail.PairingTimeOut = DateTime.UtcNow.AddMinutes(30);
                }

                if (orderStatusDetail.Status == OrderStatus.WaitingForPayment
                    && DateTime.UtcNow > orderStatusDetail.PairingTimeOut)
                {
                    orderStatusDetail.Status = OrderStatus.Completed;
                    orderStatusDetail.PairingError = "Timed out period reached while waiting for payment informations from IBS";
                }

                return;
            }
  
            // We received a fare from IBS
            // Send payment for capture, once it's captured, we will set the status to Completed
            var meterAmount = ibsOrderInfo.Fare + ibsOrderInfo.Toll + ibsOrderInfo.VAT;
            var tipAmount = GetTipAmount(meterAmount, pairingInfo.AutoTipPercentage.Value);
            _paymentService.PreAuthorizeAndCommitPayment(new PreAuthorizeAndCommitPaymentRequest
            {
                OrderId = orderStatusDetail.OrderId,
                CardToken = pairingInfo.TokenOfCardToBeUsedForPayment,
                MeterAmount = Convert.ToDecimal(meterAmount),
                TipAmount = Convert.ToDecimal(tipAmount),
                Amount = Convert.ToDecimal(meterAmount + tipAmount)
            });

            // whether there's a success or not, we change the status back to Completed since we can't process the payment again
            orderStatusDetail.Status = OrderStatus.Completed;
        }

        private double GetTipAmount(double amount, double percentage)
        {
            var tip = percentage / 100;
            return Math.Round(amount*tip, 2);
        }

        private void CheckForPairingAndHandleIfNecessary(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo)
        {
            var pairingInfo = _orderDao.FindOrderPairingById(orderStatusDetail.OrderId);
            if (pairingInfo == null)
            {
                return;
            }

            var paymentMode = _configurationManager.GetPaymentSettings().PaymentMode;
            switch (paymentMode)
            {
                case PaymentMethod.Cmt:
                case PaymentMethod.Braintree:
                case PaymentMethod.Moneris:
                    HandlePairingForStandardPairing(orderStatusDetail, pairingInfo, ibsOrderInfo);
                    break;
                case PaymentMethod.RideLinqCmt:
                    HandlePairingForRideLinqCmt(pairingInfo, ibsOrderInfo);
                    break;
                case PaymentMethod.None:
                case PaymentMethod.Fake:
                    throw new NotImplementedException("Cannot have pairing without any payment mode");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Update(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            UpdateVehiclePositionIfNecessary(ibsOrderInfo, orderStatusDetail);


            //todo rename order needs update
            if (!OrderWasUpdated(ibsOrderInfo, orderStatusDetail))
            {
                return;
            }
            
            // populate orderStatusDetail with ibsOrderInfo data
            //todo find a better term
            ibsOrderInfo.Update(orderStatusDetail);
            UpdateStatusIfNecessary(orderStatusDetail, ibsOrderInfo);

            CheckForPairingAndHandleIfNecessary(orderStatusDetail, ibsOrderInfo);
            
            //todo: merge and rename
            orderStatusDetail.IBSStatusDescription = GetDescription(orderStatusDetail.OrderId, ibsOrderInfo);
            orderStatusDetail.FareAvailable = GetFareAvailable(orderStatusDetail.OrderId, ibsOrderInfo);

            // be careful, orderStatusDetail will be directly automapped to the database entry
            // so if you send another command or modify orderStatusDetail directly, make sure you also set
            // the value in orderStatusDetail before sending this command

            //todo : remove orderstatus detail from command
            _commandBus.Send(new ChangeOrderStatus
            {
                Status = orderStatusDetail,
                Fare = ibsOrderInfo.Fare,
                Toll = ibsOrderInfo.Toll,
                Tip = ibsOrderInfo.Tip,
                Tax = ibsOrderInfo.VAT,
            });
        }

        private void UpdateStatusIfNecessary(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo)
        {
            if (orderStatusDetail.Status == OrderStatus.WaitingForPayment)
            {
                // We don't want to update since it's a special case outside of ibs
                return;
            }

            if (ibsOrderInfo.IsCanceled)
            {
                orderStatusDetail.Status = OrderStatus.Canceled;
            }
            else if (ibsOrderInfo.IsTimedOut)
            {
                orderStatusDetail.Status = OrderStatus.TimedOut;
            }
            else if (ibsOrderInfo.IsComplete)
            {
                orderStatusDetail.Status = OrderStatus.Completed;
            }
        }

        private bool GetFareAvailable(Guid orderId, IBSOrderInformation ibsOrderInfo)
        {
            var fareAvailable = ibsOrderInfo.Fare > 0;
            var payment = _orderPaymentDao.FindByOrderId(orderId);
            if (payment != null)
            {
                fareAvailable = true;
            }
            return fareAvailable;
        }

        private bool OrderWasUpdated(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            return (ibsOrderInfo.Status.HasValue() && orderStatusDetail.IBSStatusId != ibsOrderInfo.Status) // ibs status changed
                   || (!orderStatusDetail.FareAvailable && ibsOrderInfo.Fare > 0) // fare was not available and ibs now has the information
                   || orderStatusDetail.Status == OrderStatus.WaitingForPayment; // special case for pairing
        }

        private string GetDescription(Guid orderId, IBSOrderInformation ibsOrderInfo)
        {
            var orderDetail = _orderDao.FindById(orderId);
            var languageCode = orderDetail != null ? orderDetail.ClientLanguageCode : "en";

            string description = null;
            if (ibsOrderInfo.IsAssigned)
            {
                description = string.Format(_resources.Get("OrderStatus_CabDriverNumberAssigned", languageCode), ibsOrderInfo.VehicleNumber);

                if (ibsOrderInfo.Eta.HasValue)
                {
                    description += " - " + string.Format(_resources.Get("OrderStatus_CabDriverETA", languageCode), ibsOrderInfo.Eta.Value.ToString("t"));
                }
            }
            else if (ibsOrderInfo.IsCanceled)
            {
                description = _resources.Get("OrderStatus_" + ibsOrderInfo.Status, languageCode);
            }
            else if (ibsOrderInfo.IsComplete)
            {
                //FormatPrice
                var total =
                    Params.Get(ibsOrderInfo.Toll, ibsOrderInfo.Fare, ibsOrderInfo.Tip, ibsOrderInfo.VAT)
                            .Select(amount => amount)
                            .Sum();

                description = total > 0
                    ? string.Format(_resources.Get("OrderStatus_OrderDoneFareAvailable", languageCode), FormatPrice(total))
                    : _resources.Get("OrderStatus_wosDONE", languageCode);
            }

            return description.HasValue()
                        ? description
                        : _resources.Get("OrderStatus_" + ibsOrderInfo.Status, languageCode);
        }

        private string FormatPrice(double? price)
        {
            var culture = _configurationManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }
    }
}