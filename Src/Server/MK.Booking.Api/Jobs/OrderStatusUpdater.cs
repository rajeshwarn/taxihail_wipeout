﻿#region

using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
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
using apcurium.MK.Common.Resources;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using System;
using CMTPayment;
using log4net;
using ServiceStack.Common.Web;

#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class OrderStatusUpdater
    {
        private const string FailedCode = "0";

        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly IOrderDao _orderDao;
        private readonly INotificationService _notificationService;
        private readonly IDirections _directions;
        private readonly IIbsOrderService _ibsOrderService;
        private readonly IAccountDao _accountDao;
        private readonly IIbsOrderService _ibs;
        private readonly IPromotionDao _promotionDao;
        private readonly IEventSourcedRepository<Promotion> _promoRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ILogger _logger;
        private readonly Resources.Resources _resources;

        private CmtTripInfoServiceHelper _cmtTripInfoServiceHelper;

        private static readonly ILog Log = LogManager.GetLogger(typeof(OrderStatusUpdater));

        private string _languageCode = string.Empty;

        public OrderStatusUpdater(IServerSettings serverSettings, 
            ICommandBus commandBus, 
            IOrderPaymentDao paymentDao, 
            IOrderDao orderDao,
            INotificationService notificationService,
            IDirections directions,
            IIbsOrderService ibsOrderService,
            IAccountDao accountDao,
            IIbsOrderService ibs,
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository,
            IPaymentService paymentService,
            ICreditCardDao creditCardDao,
            ILogger logger)
        {
            _orderDao = orderDao;
            _notificationService = notificationService;
            _directions = directions;
            _ibsOrderService = ibsOrderService;
            _serverSettings = serverSettings;
            _accountDao = accountDao;
            _ibs = ibs;
            _promotionDao = promotionDao;
            _promoRepository = promoRepository;
            _paymentService = paymentService;
            _creditCardDao = creditCardDao;
            _logger = logger;
            _commandBus = commandBus;
            _paymentDao = paymentDao;
            _resources = new Resources.Resources(serverSettings);
        }

        public void Update(IBSOrderInformation orderFromIbs, OrderStatusDetail orderStatusDetail)
        {
            UpdateVehiclePositionAndSendNearbyNotificationIfNecessary(orderFromIbs, orderStatusDetail);

            SendUnpairWarningNotificationIfNecessary(orderStatusDetail);

            if (orderFromIbs.IsWaitingToBeAssigned)
            {
                CheckForOrderTimeOut(orderStatusDetail);
            }

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

        public void HandleManualRidelinqFlow(OrderStatusDetail orderstatusDetail)
        {
            var rideLinqDetails = _orderDao.GetManualRideLinqById(orderstatusDetail.OrderId);
            if (rideLinqDetails == null)
            {
                Log.WarnFormat("No manual RideLinQ details found for order {0}", orderstatusDetail.OrderId);
                return;
            }
            Log.DebugFormat("Initializing CmdClient for order {0} (RideLinq Pairing Code: {1})", orderstatusDetail.OrderId, rideLinqDetails.PairingToken);

            InitializeCmtServiceClient();

            var tripInfo = _cmtTripInfoServiceHelper.GetTripInfo(rideLinqDetails.PairingToken);
            if (tripInfo != null)
            {
                
                Log.DebugFormat("Sending Trip update command for trip {0} (order {1}; pairing token {2})", tripInfo.TripId, orderstatusDetail.OrderId, rideLinqDetails.PairingToken);

                Log.DebugFormat("Trip end time is {0}.", tripInfo.EndTime.HasValue ? tripInfo.EndTime.Value.ToString(CultureInfo.CurrentCulture) : "Not set yet");

                _commandBus.Send(new UpdateTripInfoInOrderForManualRideLinq
                {
                    Distance = tripInfo.Distance,
                    EndTime = tripInfo.EndTime,
                    Extra = Math.Round(((double)tripInfo.Extra / 100), 2),
                    Fare = Math.Round(((double)tripInfo.Fare / 100), 2),
                    Tax = Math.Round(((double)tripInfo.Tax / 100), 2),
                    Tip = Math.Round(((double)tripInfo.Tip / 100), 2),
                    Toll = tripInfo.TollHistory.Sum(toll => Math.Round(((double)toll.TollAmount / 100), 2)),
                    Surcharge = Math.Round(((double)tripInfo.Surcharge / 100), 2),
                    Total = Math.Round(((double)tripInfo.Total / 100), 2),
                    FareAtAlternateRate = Math.Round(((double)tripInfo.FareAtAlternateRate / 100), 2),
                    Medallion = tripInfo.Medallion,
                    RateAtTripStart = Math.Round(((double)tripInfo.RateAtTripStart / 100), 2),
                    RateAtTripEnd = Math.Round(((double)tripInfo.RateAtTripEnd / 100), 2),
                    RateChangeTime = tripInfo.RateChangeTime,
                    OrderId = orderstatusDetail.OrderId,
                    PairingToken = tripInfo.PairingToken,
                    TripId = tripInfo.TripId,
                    DriverId = tripInfo.DriverId
                });
            }
            else
            {
                Log.WarnFormat("No Trip information found for order {0} (pairing token {1})", orderstatusDetail.OrderId, rideLinqDetails.PairingToken);
            }
        }

        private void PopulateFromIbsOrder(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo)
        {
            orderStatusDetail.IBSStatusId =                     ibsOrderInfo.Status;
            orderStatusDetail.DriverInfos.FirstName =           ibsOrderInfo.FirstName.GetValue(orderStatusDetail.DriverInfos.FirstName);
            orderStatusDetail.DriverInfos.LastName =            ibsOrderInfo.LastName.GetValue(orderStatusDetail.DriverInfos.LastName);
            orderStatusDetail.DriverInfos.MobilePhone =         ibsOrderInfo.MobilePhone.GetValue(orderStatusDetail.DriverInfos.MobilePhone);
            orderStatusDetail.DriverInfos.VehicleColor =        ibsOrderInfo.VehicleColor.GetValue(orderStatusDetail.DriverInfos.VehicleColor);
            orderStatusDetail.DriverInfos.VehicleMake =         ibsOrderInfo.VehicleMake.GetValue(orderStatusDetail.DriverInfos.VehicleMake);
            orderStatusDetail.DriverInfos.VehicleModel =        ibsOrderInfo.VehicleModel.GetValue(orderStatusDetail.DriverInfos.VehicleModel);
            orderStatusDetail.DriverInfos.VehicleRegistration = ibsOrderInfo.VehicleRegistration.GetValue(orderStatusDetail.DriverInfos.VehicleRegistration);
            orderStatusDetail.DriverInfos.VehicleType =         ibsOrderInfo.VehicleType.GetValue(orderStatusDetail.DriverInfos.VehicleType);
            orderStatusDetail.DriverInfos.DriverId =            ibsOrderInfo.DriverId.GetValue(orderStatusDetail.DriverInfos.DriverId);
            orderStatusDetail.VehicleNumber =                   ibsOrderInfo.VehicleNumber.GetValue(orderStatusDetail.VehicleNumber);
            orderStatusDetail.TerminalId =                      ibsOrderInfo.TerminalId.GetValue(orderStatusDetail.TerminalId);
            orderStatusDetail.ReferenceNumber =                 ibsOrderInfo.ReferenceNumber.GetValue(orderStatusDetail.ReferenceNumber);
            orderStatusDetail.Eta =                             ibsOrderInfo.Eta ?? orderStatusDetail.Eta;
            
            UpdateStatusIfNecessary(orderStatusDetail, ibsOrderInfo);

            orderStatusDetail.IBSStatusDescription = GetDescription(orderStatusDetail.OrderId, ibsOrderInfo, orderStatusDetail.CompanyName);
        }

        private void UpdateStatusIfNecessary(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo)
        {
            if (orderStatusDetail.Status == OrderStatus.WaitingForPayment
                || (orderStatusDetail.Status == OrderStatus.TimedOut && ibsOrderInfo.IsWaitingToBeAssigned))
            {
                Log.DebugFormat("Order {1}: Status is: {0}. Don't update since it's a special case outside of IBS.", orderStatusDetail.Status, orderStatusDetail.OrderId);
                return;
            }

            if (orderStatusDetail.Status == OrderStatus.TimedOut && !ibsOrderInfo.IsWaitingToBeAssigned)
            {
                // Ride was assigned while waiting for user input on whether or not to switch company
                orderStatusDetail.Status = OrderStatus.Created;
            }
            
            if (ibsOrderInfo.IsCanceled)
            {
                orderStatusDetail.Status = OrderStatus.Canceled;
                ChargeNoShowFeeIfNecessary(ibsOrderInfo, orderStatusDetail);
                Log.DebugFormat("Order {1}: Status updated to: {0}", orderStatusDetail.Status, orderStatusDetail.OrderId);
            }
            else if (ibsOrderInfo.IsTimedOut)
            {
                orderStatusDetail.Status = OrderStatus.TimedOut;
                Log.DebugFormat("Order {1}: Status updated to: {0}", orderStatusDetail.Status, orderStatusDetail.OrderId);
            }
            else if (ibsOrderInfo.IsComplete)
            {
                orderStatusDetail.Status = OrderStatus.Completed;
                Log.DebugFormat("Order {1}: Status updated to: {0}", orderStatusDetail.Status, orderStatusDetail.OrderId);
            }
        }

        private PreAuthorizePaymentResponse PreauthorizePaymentIfNecessary(Guid orderId, decimal amount)
        {
            // Check payment instead of PreAuth setting, because we do not preauth in the cases of future bookings
            var paymentInfo = _paymentDao.FindByOrderId(orderId);
            if (paymentInfo != null)
            {
                // Already preauthorized on create order, do nothing
                return new PreAuthorizePaymentResponse { IsSuccessful = true };
            }

            var orderDetail = _orderDao.FindById(orderId);
            if (orderDetail == null)
            {
                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = false, 
                    Message = "Order not found"
                };
            }

            var account = _accountDao.FindById(orderDetail.AccountId);

            var result = _paymentService.PreAuthorize(orderId, account, amount);

            if (result.IsSuccessful)
            {
                // Wait for OrderPaymentDetail to be created
                Thread.Sleep(500);
            }
            else if (result.IsDeclined)
            {
                // Deactivate credit card if it was declined
                _commandBus.Send(new ReactToPaymentFailure
                {
                    AccountId = orderDetail.AccountId,
                    OrderId = orderId,
                    IBSOrderId = orderDetail.IBSOrderId,
                    OverdueAmount = amount,
                    TransactionId = result.TransactionId,
                    TransactionDate = result.TransactionDate
                });
            }

            return result;
        }

        private void ChargeNoShowFeeIfNecessary(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            if (ibsOrderInfo.Status != VehicleStatuses.Common.NoShow)
            {
                return;
            }

            // Order is prepaid, if the user prepaid and decided not to show up, the fee is his fare already charged
            if (orderStatusDetail.IsPrepaid)
            {
                return;
            }

            Log.DebugFormat("No show fee will be charged for order {0}.", ibsOrderInfo.IBSOrderId);

            var paymentSettings = _serverSettings.GetPaymentSettings();
            var account = _accountDao.FindById(orderStatusDetail.AccountId);

            if (paymentSettings.NoShowFee.HasValue
                && paymentSettings.NoShowFee.Value > 0
                && (account.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                    || account.Settings.ChargeTypeId == ChargeTypes.PayPal.Id))
            {
                try
                {
                    // PreAuthorization
                    var preAuthResponse = PreauthorizePaymentIfNecessary(orderStatusDetail.OrderId, paymentSettings.NoShowFee.Value);
                    if (preAuthResponse.IsSuccessful)
                    {
                        // Commit
                        var paymentResult = CommitPayment(paymentSettings.NoShowFee.Value, paymentSettings.NoShowFee.Value, 0, orderStatusDetail.OrderId, true);
                        if (paymentResult.IsSuccessful)
                        {
                            Log.DebugFormat("No show fee of amount {0} was charged for order {1}.", paymentSettings.NoShowFee.Value, ibsOrderInfo.IBSOrderId);
                        }
                        else
                        {
                            orderStatusDetail.PairingError = paymentResult.Message;
                            Log.DebugFormat("Could not process no show fee for order {0}: {1}.", ibsOrderInfo.IBSOrderId, paymentResult.Message);
                        }
                    }
                    else
                    {
                        orderStatusDetail.PairingError = preAuthResponse.Message;
                        Log.DebugFormat("Could not process no show fee for order {0}: {1}.", ibsOrderInfo.IBSOrderId, preAuthResponse.Message);
                    }
                }
                catch (Exception ex)
                {
                    orderStatusDetail.PairingError = ex.Message;
                    Log.DebugFormat("Could not process no show fee for order {0}: {1}.", ibsOrderInfo.IBSOrderId, ex.Message);
                }
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

        private void SendUnpairWarningNotificationIfNecessary(OrderStatusDetail orderStatus)
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();
            if (!paymentSettings.IsUnpairingDisabled && orderStatus.UnpairingTimeOut.HasValue)
            {
                var halfwayUnpairTimeout = orderStatus.UnpairingTimeOut.Value.AddSeconds(-0.5 * paymentSettings.UnpairingTimeOut);

                if (DateTime.UtcNow >= halfwayUnpairTimeout)
                {
                    // Send unpair timeout reminder halfway through
                    _notificationService.SendUnpairingReminderPush(orderStatus.OrderId);
                }
            }
        }

        private void HandlePairingForRideLinqCmt(OrderPairingDetail pairingInfo, IBSOrderInformation ibsOrderInfo)
        {
            // in the case of RideLinq CMT, we only want to calculate the tip to fill information on our side
            if (pairingInfo.AutoTipPercentage.HasValue)
            {
                ibsOrderInfo.Tip = FareHelper.CalculateTipAmount(ibsOrderInfo.Fare, pairingInfo.AutoTipPercentage.Value);
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
            var orderPayment = _paymentDao.FindByOrderId(orderStatusDetail.OrderId);
            if (orderPayment != null && (orderPayment.IsCompleted || orderPayment.IsCancelled))
            {
                // Payment was already processed
                Log.DebugFormat("Payment for order {0} was already processed, nothing else to do.", orderStatusDetail.OrderId);
                return;
            }

            if (ibsOrderInfo.IsMeterOffNotPaid)
            {
                SendPaymentBeingProcessedMessageToDriver(ibsOrderInfo.VehicleNumber);
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
                    Log.DebugFormat("Order {1}: Status updated to: {0} with timeout in 30 minutes", orderStatusDetail.Status, orderStatusDetail.OrderId);
                }

                if (orderStatusDetail.Status == OrderStatus.WaitingForPayment
                    && DateTime.UtcNow > orderStatusDetail.PairingTimeOut)
                {
                    orderStatusDetail.Status = OrderStatus.Completed;
                    _paymentService.VoidPreAuthorization(orderStatusDetail.OrderId);

                    orderStatusDetail.PairingError = "Timed out period reached while waiting for payment informations from IBS.";
                    Log.ErrorFormat("Order {1}: Pairing error: {0}", orderStatusDetail.PairingError, orderStatusDetail.OrderId);
                }

                return;
            }

            // We received a fare from IBS
            // Send payment for capture, once it's captured, we will set the status to Completed
            var meterAmount = ibsOrderInfo.Fare + ibsOrderInfo.Toll + ibsOrderInfo.VAT;
            double tipPercentage = pairingInfo.AutoTipPercentage ?? _serverSettings.ServerData.DefaultTipPercentage;
            var tipAmount = FareHelper.CalculateTipAmount(meterAmount, tipPercentage);

            Log.DebugFormat(
                    "Order {4}: Received total amount from IBS of {0}, calculated a tip of {1}% (tip amount: {2}), for a total of {3}",
                    meterAmount, tipPercentage, tipAmount, meterAmount + tipAmount, orderStatusDetail.OrderId);

            if (!_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
            {
                // this is the only payment related message sent to the driver when this setting is false
                SendMinimalPaymentProcessedMessageToDriver(ibsOrderInfo.VehicleNumber, meterAmount + tipAmount, meterAmount, tipAmount);
            }

            try
            {
                var totalOrderAmount = Convert.ToDecimal(meterAmount + tipAmount);
                var amountSaved = 0m;

                var promoUsed = _promotionDao.FindByOrderId(orderStatusDetail.OrderId);
                if (promoUsed != null)
                {
                    var promoDomainObject = _promoRepository.Get(promoUsed.PromoId);
                    amountSaved = promoDomainObject.GetAmountSaved(Convert.ToDecimal(meterAmount));
                    totalOrderAmount = totalOrderAmount - amountSaved;
                }
                
                // Preautorize
                var preAuthResponse = PreauthorizePaymentIfNecessary(orderStatusDetail.OrderId, totalOrderAmount);
                if (preAuthResponse.IsSuccessful)
                {
                    // Commit
                    var paymentResult = CommitPayment(
                        totalOrderAmount, 
                        Convert.ToDecimal(meterAmount), 
                        Convert.ToDecimal(tipAmount), 
                        orderStatusDetail.OrderId, 
                        false,
                        promoUsed != null
                            ? promoUsed.PromoId
                            : (Guid?) null,
                        amountSaved);
                    if (paymentResult.IsSuccessful)
                    {
                        Log.DebugFormat("Order {0}: Payment Successful (Auth: {1}) [Transaction Id: {2}]", orderStatusDetail.OrderId, paymentResult.AuthorizationCode, paymentResult.TransactionId);
                    }
                    else
                    {
                        if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
                        {
                            _ibsOrderService.SendMessageToDriver(_resources.Get("PaymentFailedToDriver"), orderStatusDetail.VehicleNumber);
                        }

                        // set the payment error message in OrderStatusDetail for reporting purpose
                        orderStatusDetail.PairingError = paymentResult.Message;

                        Log.ErrorFormat("Order {0}: Payment FAILED (Message: {1}) [Transaction Id: {2}]", orderStatusDetail.OrderId, paymentResult.Message, paymentResult.TransactionId);
                    }
                }
                else
                {
                    if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
                    {
                        _ibsOrderService.SendMessageToDriver(_resources.Get("PaymentFailedToDriver"), orderStatusDetail.VehicleNumber);
                    }

                    // set the payment error message in OrderStatusDetail for reporting purpose
                    orderStatusDetail.PairingError = preAuthResponse.Message;

                    Log.ErrorFormat("Order {0}: Payment FAILED (Message: {1}) [Transaction Id: {2}]", orderStatusDetail.OrderId, preAuthResponse.Message, preAuthResponse.TransactionId);
                }
            }
            catch (Exception ex)
            {
                if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
                {
                    _ibsOrderService.SendMessageToDriver(_resources.Get("PaymentFailedToDriver"), orderStatusDetail.VehicleNumber);
                }

                // set the payment error message in OrderStatusDetail for reporting purpose
                orderStatusDetail.PairingError = ex.Message;

                Log.ErrorFormat("Order {0}: Payment FAILED (Message: {1}) [Transaction Id: {2}]", orderStatusDetail.OrderId, ex.Message, "UNKNOWN");
            }
            
            // whether there's a success or not, we change the status back to Completed since we can't process the payment again
            orderStatusDetail.Status = OrderStatus.Completed;
        }

        private CommitPreauthorizedPaymentResponse CommitPayment(decimal totalOrderAmount, decimal meterAmount, decimal tipAmount, Guid orderId, bool isNoShowFee, Guid? promoUsedId = null, decimal amountSaved = 0)
        {
            var orderDetail = _orderDao.FindById(orderId);
            if (orderDetail == null)
            {
                throw new Exception("Order not found");
            }

            if (orderDetail.IBSOrderId == null)
            {
                throw new Exception("Order has no IBSOrderId");
            }

            var account = _accountDao.FindById(orderDetail.AccountId);

            var paymentDetail = _paymentDao.FindByOrderId(orderId);
            if (paymentDetail == null)
            {
                throw new Exception("Payment not found");
            }

            var paymentProviderServiceResponse = new CommitPreauthorizedPaymentResponse
            {
                TransactionId = paymentDetail.TransactionId
            };

            try
            {
                var message = string.Empty;
                
                if (paymentDetail.IsCompleted)
                {
                    message = "Order already paid or payment currently processing";
                }
                else
                {
                    if (totalOrderAmount > 0)
                    {
                        paymentProviderServiceResponse = _paymentService.CommitPayment(orderId, account, paymentDetail.PreAuthorizedAmount, totalOrderAmount, meterAmount, tipAmount, paymentDetail.TransactionId);
                        message = paymentProviderServiceResponse.Message;
                    }
                    else
                    {
                        // promotion made the ride free to the user
                        // void preauth if it exists
                        _paymentService.VoidPreAuthorization(orderId);

                        paymentProviderServiceResponse.IsSuccessful = true;
                    }
                }

                if (!isNoShowFee)
                {
                    //send information to IBS
                    try
                    {
                        var providerType = _paymentService.ProviderType(orderDetail.Id);

                        string cardToken;
                        if (providerType == PaymentProvider.PayPal)
                        {
                            cardToken = "PayPal";
                        }
                        else
                        {
                            var card = _creditCardDao.FindByAccountId(orderDetail.AccountId).First();
                            cardToken = card.Token;
                        }

                        _ibs.ConfirmExternalPayment(orderDetail.Id,
                            orderDetail.IBSOrderId.Value,
                            totalOrderAmount,
                            Convert.ToDecimal(tipAmount),
                            Convert.ToDecimal(meterAmount),
                            paymentProviderServiceResponse.IsSuccessful ? PaymentType.CreditCard.ToString() : FailedCode,
                            providerType.ToString(),
                            paymentProviderServiceResponse.TransactionId,
                            paymentProviderServiceResponse.AuthorizationCode,
                            cardToken,
                            account.IBSAccountId.Value,
                            orderDetail.Settings.Name,
                            orderDetail.Settings.Phone,
                            account.Email,
                            orderDetail.UserAgent.GetOperatingSystem(),
                            orderDetail.UserAgent);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e);
                        message = e.Message;

                        try
                        {
                            if (paymentProviderServiceResponse.IsSuccessful)
                            {
                                _paymentService.VoidTransaction(orderId, paymentProviderServiceResponse.TransactionId, ref message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogMessage("Can't cancel transaction");
                            _logger.LogError(ex);
                            message = message + ex.Message;
                            //can't cancel transaction, send a command to log later
                        }
                        finally
                        {
                            paymentProviderServiceResponse.IsSuccessful = false;
                        }
                    }
                }

                if (paymentProviderServiceResponse.IsSuccessful)
                {
                    // Payment completed

                    var fareObject = FareHelper.GetFareFromAmountInclTax(Convert.ToDouble(meterAmount), _serverSettings.ServerData.VATIsEnabled ? _serverSettings.ServerData.VATPercentage : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        AccountId = account.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(orderDetail.Id),
                        Amount = totalOrderAmount,
                        MeterAmount = Convert.ToDecimal(fareObject.AmountExclTax),
                        TipAmount = Convert.ToDecimal(tipAmount),
                        TaxAmount = Convert.ToDecimal(fareObject.TaxAmount),
                        IsNoShowFee = isNoShowFee,
                        AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                        TransactionId = paymentProviderServiceResponse.TransactionId,
                        PromotionUsed = promoUsedId,
                        AmountSavedByPromotion = amountSaved
                    });
                }
                else
                {
                    // Void PreAuth because commit failed
                    _paymentService.VoidPreAuthorization(orderId);

                    // Payment error
                    _commandBus.Send(new LogCreditCardError
                    {
                        PaymentId = paymentDetail.PaymentId,
                        Reason = message
                    });

                    if (paymentProviderServiceResponse.IsDeclined)
                    {
                        _commandBus.Send(new ReactToPaymentFailure
                        {
                            AccountId = account.Id,
                            OrderId = orderId,
                            IBSOrderId = orderDetail.IBSOrderId,
                            OverdueAmount = totalOrderAmount,
                            TransactionId = paymentProviderServiceResponse.TransactionId,
                            TransactionDate = paymentProviderServiceResponse.TransactionDate
                        });
                    }
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                    TransactionId = paymentProviderServiceResponse.TransactionId,
                    IsSuccessful = paymentProviderServiceResponse.IsSuccessful,
                    Message = paymentProviderServiceResponse.IsSuccessful ? "Success" : message
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("Error during payment " + e);
                _logger.LogError(e);
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = paymentProviderServiceResponse.TransactionId,
                    Message = e.Message
                };
            }
        }

        private void CheckForPairingAndHandleIfNecessary(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo)
        {
            if (orderStatusDetail.IsPrepaid)
            {
                Log.DebugFormat("Order {0}: No pairing to process as the order has been paid at the time of booking.", orderStatusDetail.OrderId);
                return;
            }

            var pairingInfo = _orderDao.FindOrderPairingById(orderStatusDetail.OrderId);
            if (pairingInfo == null)
            {
                Log.DebugFormat("Order {0}: No pairing to process as no pairing information was found.", orderStatusDetail.OrderId);
                return;
            }

            var paymentMode = _serverSettings.GetPaymentSettings().PaymentMode;
            var isPayPal = _paymentService.IsPayPal(null, orderStatusDetail.OrderId);
            
            if (!isPayPal && paymentMode == PaymentMethod.RideLinqCmt)
            {
                HandlePairingForRideLinqCmt(pairingInfo, ibsOrderInfo);
                return;
            }

            if (isPayPal
                || paymentMode == PaymentMethod.Cmt
                || paymentMode == PaymentMethod.Braintree
                || paymentMode == PaymentMethod.Moneris)
            {
                HandlePairingForStandardPairing(orderStatusDetail, pairingInfo, ibsOrderInfo);
                return;
            }
            
            throw new NotImplementedException("Cannot have pairing without any payment mode");
        }

        private bool OrderNeedsUpdate(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            return (ibsOrderInfo.Status.HasValue()                                // ibs status changed
                        && orderStatusDetail.IBSStatusId != ibsOrderInfo.Status) 
                   || (!orderStatusDetail.FareAvailable                           // fare was not available and ibs now has the information
                        && ibsOrderInfo.Fare > 0) 
                   || orderStatusDetail.Status == OrderStatus.WaitingForPayment   // special case for pairing
                   || (orderStatusDetail.Status == OrderStatus.TimedOut           // special case for network
                        && _serverSettings.ServerData.Network.Enabled);           
        }

        private void CheckForOrderTimeOut(OrderStatusDetail orderStatusDetail)
        {
            if (!_serverSettings.ServerData.Network.Enabled
                || orderStatusDetail.Status == OrderStatus.TimedOut
                || orderStatusDetail.IgnoreDispatchCompanySwitch)
            {
                // Nothing to do
                return;
            }

            if (orderStatusDetail.NetworkPairingTimeout.HasValue
                && orderStatusDetail.NetworkPairingTimeout.Value <= DateTime.UtcNow)
            {
                // Order timed out
                _commandBus.Send(new NotifyOrderTimedOut
                {
                    OrderId = orderStatusDetail.OrderId,
                    Market = orderStatusDetail.Market
                });
            }
        }

        private string GetDescription(Guid orderId, IBSOrderInformation ibsOrderInfo, string companyName)
        {
            var orderDetail = _orderDao.FindById(orderId);
            _languageCode = orderDetail != null ? orderDetail.ClientLanguageCode : SupportedLanguages.en.ToString();

            string description = null;
            if (ibsOrderInfo.IsWaitingToBeAssigned)
            {
                if (companyName.HasValue())
                {
                    description = string.Format(_resources.Get("OrderStatus_wosWAITINGRoaming", _languageCode), companyName);
                    Log.DebugFormat("Setting Waiting in roaming status description: {0}", description);
                }
            }
            else if (ibsOrderInfo.IsAssigned)
            {
                description = string.Format(_resources.Get("OrderStatus_CabDriverNumberAssigned", _languageCode), ibsOrderInfo.VehicleNumber);
                Log.DebugFormat("Setting Assigned status description: {0}", description);

                if (_serverSettings.ServerData.ShowEta)
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
                description = _resources.Get("OrderStatus_wosDONE", _languageCode);    
                Log.DebugFormat("Setting Complete status description: {0}", description);
            }
            else if (ibsOrderInfo.IsLoaded)
            {
                if (orderDetail != null 
                    && _serverSettings.GetPaymentSettings().IsUnpairingDisabled
                    && (orderDetail.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                        || orderDetail.Settings.ChargeTypeId == ChargeTypes.PayPal.Id))
                {
                    description = _resources.Get("OrderStatus_wosLOADEDAutoPairing", _languageCode);
                }
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
                var etaMessage = string.Format(_resources.Get("EtaMessageToDriver"), eta.FormattedDistance, eta.Duration);
                _ibsOrderService.SendMessageToDriver(etaMessage, vehicleNumber);
                Log.Debug(etaMessage);
            }
        }

        private void SendPaymentBeingProcessedMessageToDriver(string vehicleNumber)
        {
            var paymentBeingProcessedMessage = _resources.Get("PaymentBeingProcessedMessageToDriver");
            _ibsOrderService.SendMessageToDriver(paymentBeingProcessedMessage, vehicleNumber);
            Log.Debug(paymentBeingProcessedMessage);
        }

        private void SendMinimalPaymentProcessedMessageToDriver(string vehicleNumber, double amount, double meter, double tip)
        {
            _ibsOrderService.SendPaymentNotification(amount, meter, tip, null, vehicleNumber);
        }

        private void InitializeCmtServiceClient()
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);
        }
    }
}