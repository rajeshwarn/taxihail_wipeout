#region

using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using System;
using log4net;

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
        private readonly IDirections _directions;
        private readonly IIbsOrderService _ibsOrderService;
        private readonly Resources.Resources _resources;
        private readonly IAppSettings _appSettings;

        private static readonly ILog Log = LogManager.GetLogger(typeof(CreateOrderService));

        private string _languageCode = "";

        public OrderStatusUpdater(IConfigurationManager configurationManager, 
            ICommandBus commandBus, 
            IOrderPaymentDao orderPaymentDao, 
            IOrderDao orderDao,
            IPaymentService paymentService,
            INotificationService notificationService,
            IDirections directions,
            IAppSettings appSettings,
            IIbsOrderService ibsOrderService)
        {
            _appSettings = appSettings;
            _orderDao = orderDao;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _directions = directions;
            _ibsOrderService = ibsOrderService;
            _configurationManager = configurationManager;
            _commandBus = commandBus;
            _orderPaymentDao = orderPaymentDao;

            _resources = new Resources.Resources(configurationManager.GetSetting("TaxiHail.ApplicationKey"), appSettings);
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
                Log.DebugFormat("Order Status is: {0}. Don't update since it's a special case outside of IBS.", orderStatusDetail.Status);
                return;
            }
            if (ibsOrderInfo.IsCanceled)
            {
                orderStatusDetail.Status = OrderStatus.Canceled;
                Log.DebugFormat("Order Status updated to: {0}", orderStatusDetail.Status);
            }
            else if (ibsOrderInfo.IsTimedOut)
            {
                orderStatusDetail.Status = OrderStatus.TimedOut;
                Log.DebugFormat("Order Status updated to: {0}", orderStatusDetail.Status);
            }
            else if (ibsOrderInfo.IsComplete)
            {
                orderStatusDetail.Status = OrderStatus.Completed;
                Log.DebugFormat("Order Status updated to: {0}", orderStatusDetail.Status);
            }
        }

        private void UpdateVehiclePositionAndSendNearbyNotificationIfNecessary(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatus)
        {
            if (orderStatus.VehicleLatitude != ibsOrderInfo.VehicleLatitude
                || orderStatus.VehicleLongitude != ibsOrderInfo.VehicleLongitude)
            {
                _orderDao.UpdateVehiclePosition(orderStatus.OrderId, ibsOrderInfo.VehicleLatitude, ibsOrderInfo.VehicleLongitude);
                _notificationService.SendTaxiNearbyPush(orderStatus.OrderId, ibsOrderInfo.Status, ibsOrderInfo.VehicleLatitude, ibsOrderInfo.VehicleLongitude);

                Log.DebugFormat("Vehicle position updated. New position: ({0}, {1}).", ibsOrderInfo.VehicleLatitude, ibsOrderInfo.VehicleLongitude);
            }
        }

        private void HandlePairingForRideLinqCmt(OrderPairingDetail pairingInfo, IBSOrderInformation ibsOrderInfo)
        {
            // in the case of RideLinq CMT, we only want to calculate the tip to fill information on our side
            if (pairingInfo.AutoTipPercentage.HasValue)
            {
                ibsOrderInfo.Tip = GetTipAmount(ibsOrderInfo.Fare, pairingInfo.AutoTipPercentage.Value);
                Log.DebugFormat("RideLinqCmt Pairing: Calculated a tip amount of {0}, based on an auto AutoTipPercentage percentage of {1}",
                    ibsOrderInfo.Tip, pairingInfo.AutoTipPercentage.Value);
            }
            else
            {
                Log.Debug("RideLinqCmt Pairing: AutoTipPercentage is null, no tip amount was assigned.");
            }
        }

        private void HandlePairingForStandardPairing(OrderStatusDetail orderStatusDetail, OrderPairingDetail pairingInfo, IBSOrderInformation ibsOrderInfo)
        {
            if (!_configurationManager.GetPaymentSettings().AutomaticPayment)
            {
                Log.Debug("Standard Pairing: Automatic payment is disabled, nothing else to do.");
                return;
            }

            var orderPayment = _orderPaymentDao.FindByOrderId(orderStatusDetail.OrderId);
            if (orderPayment != null)
            {
                // Payment was already processed
                Log.DebugFormat("Payment for order {0} was already processed, nothing else to do.", orderStatusDetail.OrderId);
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
                    orderStatusDetail.PairingError = "Timed out period reached while waiting for payment informations from IBS.";
                    Log.ErrorFormat("Pairing error: {0}", orderStatusDetail.PairingError);
                }

                return;
            }

            // We received a fare from IBS
            // Send payment for capture, once it's captured, we will set the status to Completed
            var meterAmount = ibsOrderInfo.Fare + ibsOrderInfo.Toll + ibsOrderInfo.VAT;
            double tipPercentage = pairingInfo.AutoTipPercentage ?? _appSettings.Data.DefaultTipPercentage;
            var tipAmount = GetTipAmount(meterAmount, tipPercentage);

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

            Log.DebugFormat("Received total amount from IBS of {0}, calculated a tip of {1}% (tip amount: {2}), for a total of {3}",
                                            meterAmount, tipPercentage, tipAmount, meterAmount + tipAmount);
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
                Log.DebugFormat("No pairing to process for order {0} as no pairing information was found.", orderStatusDetail.OrderId);
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
            _languageCode = orderDetail != null ? orderDetail.ClientLanguageCode : "en";

            string description = null;
            if (ibsOrderInfo.IsAssigned)
            {
                description = string.Format(_resources.Get("OrderStatus_CabDriverNumberAssigned", _languageCode), ibsOrderInfo.VehicleNumber);
                Log.DebugFormat("Setting Assigned status description: {0}", description);

                if (_configurationManager.GetSetting("Client.ShowEta", false))
                {
                    try
                    {
                        SendEtaMessageToDriver((double) ibsOrderInfo.VehicleLatitude, (double) ibsOrderInfo.VehicleLongitude, 
                            orderDetail.PickupAddress.Latitude, orderDetail.PickupAddress.Longitude, ibsOrderInfo.VehicleNumber);
                    }
                    catch
                    {
                        Log.Error("Cannot Send Eta to Vehicle Number " + ibsOrderInfo.VehicleNumber);
                    }
                }
            }
            else if (ibsOrderInfo.IsCanceled)
            {
                description = _resources.Get("OrderStatus_" + ibsOrderInfo.Status, _languageCode);
                Log.DebugFormat("Setting Canceled status description: {0}", description);
            }
            else if (ibsOrderInfo.IsComplete)
            {
                //FormatPrice
                var total =
                    Params.Get(ibsOrderInfo.Toll, ibsOrderInfo.Fare, ibsOrderInfo.Tip, ibsOrderInfo.VAT)
                            .Select(amount => amount)
                            .Sum();

                description = total > 0
                    ? string.Format(_resources.Get("OrderStatus_OrderDoneFareAvailable", _languageCode), _resources.FormatPrice(total))
                    : _resources.Get("OrderStatus_wosDONE", _languageCode);
                    
               Log.DebugFormat("Setting Complete status description: {0}", description);
            }
            else if (ibsOrderInfo.IsLoaded)
            {
                if (orderDetail != null && (_configurationManager.GetPaymentSettings().AutomaticPayment
                                            && _configurationManager.GetPaymentSettings().AutomaticPaymentPairing
                                            && orderDetail.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id))
                {
                    description = _resources.Get("OrderStatus_wosLOADEDAutoPairing", _languageCode);
                }
            }
            else if (ibsOrderInfo.IsMeterOffNotPaid)
            {
                SendPayInCarMessageToDriver(ibsOrderInfo.VehicleNumber);
            }

            return description.HasValue()
                        ? description
                        : _resources.Get("OrderStatus_" + ibsOrderInfo.Status, _languageCode);
        }

        private void SendEtaMessageToDriver(double vehicleLatitude, double vehicleLongitude, double pickupLatitude, double pickupLongitude, string vehicleNumber)
        {
            var eta = _directions.GetEta(vehicleLatitude, vehicleLongitude, pickupLatitude, pickupLongitude);
            if (eta != null && eta.IsValidEta())
            {
                string etaMessage = string.Format(_resources.Get("EtaMessageToDriver", _languageCode), eta.FormattedDistance, eta.Duration);
                _ibsOrderService.SendMessageToDriver(etaMessage, vehicleNumber);
                Log.Debug(etaMessage);
            }
        }

        private void SendPayInCarMessageToDriver(string vehicleNumber)
        {
            string payInCarMessage = _resources.Get("PayInCarMessageToDriver", _languageCode);
            _ibsOrderService.SendMessageToDriver(payInCarMessage, vehicleNumber);
            Log.Debug(payInCarMessage);
        }
    }
}