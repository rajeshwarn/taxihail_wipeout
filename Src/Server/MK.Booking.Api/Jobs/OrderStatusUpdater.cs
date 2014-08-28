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
using apcurium.MK.Booking.Services;
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
        private readonly INotificationService _notificationService;
        private readonly Resources.Resources _resources;
        private IAppSettings _appSettings;

        public OrderStatusUpdater(IConfigurationManager configurationManager, 
            ICommandBus commandBus, 
            IOrderPaymentDao orderPaymentDao, 
            IOrderDao orderDao,
            IPaymentService paymentService,
            INotificationService notificationService,
            IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _orderDao = orderDao;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _configurationManager = configurationManager;
            _commandBus = commandBus;
            _orderPaymentDao = orderPaymentDao;
            _resources = new Resources.Resources(configurationManager.GetSetting("TaxiHail.ApplicationKey"));
        }

        

        public void Update(IBSOrderInformation orderFromIbs, OrderStatusDetail orderStatusDetail)
        {
            UpdateVehiclePositionAndSendNearbyNotificationIfNecessary(orderFromIbs, orderStatusDetail);
            
            if (!OrderNeedsUpdate(orderFromIbs, orderStatusDetail))
            {
                return;
            }

            PopulateFromIbsOrder(orderStatusDetail, orderFromIbs);

            CheckForPairingAndHandleIfNecessary(orderStatusDetail, orderFromIbs);

            _commandBus.Send(new ChangeOrderStatus
            {
                Status = orderStatusDetail,
                Fare = orderFromIbs.Fare,
                Toll = orderFromIbs.Toll,
                Tip = orderFromIbs.Tip,
                Tax = orderFromIbs.VAT,
            });
        }

        private void PopulateFromIbsOrder(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo)
        {
            orderStatusDetail.IBSStatusId = ibsOrderInfo.Status;
            orderStatusDetail.DriverInfos.FirstName = ibsOrderInfo.FirstName.GetValue(orderStatusDetail.DriverInfos.FirstName);
            orderStatusDetail.DriverInfos.LastName = ibsOrderInfo.LastName.GetValue(orderStatusDetail.DriverInfos.LastName);
            orderStatusDetail.DriverInfos.MobilePhone = ibsOrderInfo.MobilePhone.GetValue(orderStatusDetail.DriverInfos.MobilePhone);
            orderStatusDetail.DriverInfos.VehicleColor = ibsOrderInfo.VehicleColor.GetValue(orderStatusDetail.DriverInfos.VehicleColor);
            orderStatusDetail.DriverInfos.VehicleMake = ibsOrderInfo.VehicleMake.GetValue(orderStatusDetail.DriverInfos.VehicleMake);
            orderStatusDetail.DriverInfos.VehicleModel = ibsOrderInfo.VehicleModel.GetValue(orderStatusDetail.DriverInfos.VehicleModel);
            orderStatusDetail.DriverInfos.VehicleRegistration = ibsOrderInfo.VehicleRegistration.GetValue(orderStatusDetail.DriverInfos.VehicleRegistration);
            orderStatusDetail.DriverInfos.VehicleType = ibsOrderInfo.VehicleType.GetValue(orderStatusDetail.DriverInfos.VehicleType);
            orderStatusDetail.DriverInfos.DriverId = ibsOrderInfo.DriverId.GetValue(orderStatusDetail.DriverInfos.DriverId);
            orderStatusDetail.VehicleNumber = ibsOrderInfo.VehicleNumber.GetValue(orderStatusDetail.VehicleNumber);
            orderStatusDetail.TerminalId = ibsOrderInfo.TerminalId.GetValue(orderStatusDetail.TerminalId);
            orderStatusDetail.ReferenceNumber = ibsOrderInfo.ReferenceNumber.GetValue(orderStatusDetail.ReferenceNumber);
            orderStatusDetail.Eta = ibsOrderInfo.Eta ?? orderStatusDetail.Eta;

            UpdateStatusIfNecessary(orderStatusDetail, ibsOrderInfo);

            orderStatusDetail.IBSStatusDescription = GetDescription(orderStatusDetail.OrderId, ibsOrderInfo);
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

        private void UpdateVehiclePositionAndSendNearbyNotificationIfNecessary(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatus)
        {
            if (orderStatus.VehicleLatitude != ibsOrderInfo.VehicleLatitude
                || orderStatus.VehicleLongitude != ibsOrderInfo.VehicleLongitude)
            {
                _orderDao.UpdateVehiclePosition(orderStatus.OrderId, ibsOrderInfo.VehicleLatitude, ibsOrderInfo.VehicleLongitude);
                _notificationService.SendTaxiNearbyNotification(orderStatus.OrderId, ibsOrderInfo.Status, ibsOrderInfo.VehicleLatitude, ibsOrderInfo.VehicleLongitude);
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
            if (!_appSettings.Data.AutomaticPayment)
            {
                // Automatic payment is disabled, nothing to do here
                return;
            }

            var orderPayment = _orderPaymentDao.FindByOrderId(orderStatusDetail.OrderId);
            if (orderPayment != null)
            {
                // Payment was already processed
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
            return Math.Round(amount * tip, 2);
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

        private bool OrderNeedsUpdate(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
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
            var culture = _appSettings.Data.PriceFormat;
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }
    }
}