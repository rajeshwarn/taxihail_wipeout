#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using CMTPayment;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class MailSender : IIntegrationEventHandler,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<CreditCardDeactivated>,
        IEventHandler<UserAddedToPromotionWhiteList_V2>,
        IEventHandler<ManualRideLinqTripInfoUpdated>
    {
        private readonly ICommandBus _commandBus;
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;
        private readonly IPromotionDao _promotionDao;
        private readonly IAccountDao _accountDao;
        private readonly INotificationService _notificationService;
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;

        private CmtTripInfoServiceHelper _cmtTripInfoServiceHelper;

        public MailSender(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            ICreditCardDao creditCardDao,
            IPromotionDao promotionDao,
            IOrderDao orderDao,
            IAccountDao accountDao,
            INotificationService notificationService,
            IServerSettings serverSettings,
            ILogger logger)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _creditCardDao = creditCardDao;
            _orderDao = orderDao;
            _promotionDao = promotionDao;
            _accountDao = accountDao;
            _notificationService = notificationService;
            _serverSettings = serverSettings;
            _logger = logger;
        }

        public void Handle(OrderStatusChanged @event)
        {
            if (@event.IsCompleted)
            {
                if (@event.Status.IsPrepaid)
                {
                    // Send receipt for PrePaid
                    // No tolls and surcharge for prepaid orders
                    SendTripReceipt(@event.SourceId, 
                        Convert.ToDecimal(@event.Fare ?? 0),
                        Convert.ToDecimal(@event.Tip ?? 0),
                        Convert.ToDecimal(@event.Tax ?? 0)); 
                }
                else
                {
                    var order = _orderDao.FindById(@event.SourceId);
                    var pairingInfo = _orderDao.FindOrderPairingById(@event.SourceId);

                    if (order.Settings.ChargeTypeId == ChargeTypes.PaymentInCar.Id)
                    {
                        // Send receipt for Pay in Car
                        SendTripReceipt(@event.SourceId,
                            Convert.ToDecimal(@event.Fare ?? 0),
                            Convert.ToDecimal(@event.Tip ?? 0),
                            Convert.ToDecimal(@event.Tax ?? 0),
                            toll: Convert.ToDecimal(@event.Toll ?? 0),
                            surcharge: Convert.ToDecimal(@event.Surcharge ?? 0));
                    }
                    else if (pairingInfo != null && pairingInfo.DriverId.HasValue() && pairingInfo.Medallion.HasValue() && pairingInfo.PairingToken.HasValue())
                    {
                        // Send receipt for CMTRideLinq
                        InitializeCmtServiceClient(order.CompanyKey, order.Settings.ServiceType);

                        var tripInfo = _cmtTripInfoServiceHelper.GetTripInfo(pairingInfo.PairingToken);
                        if (tripInfo != null && !tripInfo.ErrorCode.HasValue && tripInfo.EndTime.HasValue)
                        {
							var tollHistory = tripInfo.TollHistory != null
								? tripInfo.TollHistory.Sum(p => p.TollAmount)
								: 0;

                            var meterAmount = Math.Round(((double)tripInfo.Fare / 100), 2);
                            var tollAmount = Math.Round(((double)tollHistory / 100), 2);
                            var tipAmount = Math.Round(((double)tripInfo.Tip / 100), 2);
                            var taxAmount = Math.Round(((double)tripInfo.Tax / 100), 2);
                            var surchargeAmount = Math.Round(((double)tripInfo.Surcharge / 100), 2);
                            var extraAmount = Math.Round(((double)tripInfo.Extra / 100), 2);
                            var accessFee = Math.Round(((double)tripInfo.AccessFee / 100), 2);
                            var fareAtAlternateRate = Math.Round(((double)tripInfo.FareAtAlternateRate / 100), 2);

                            var tolls = new List<TollDetail>();

                            if (tripInfo.TollHistory != null)
                            {
                                tolls.AddRange(tripInfo.TollHistory.Select(toll =>
                                    new TollDetail
                                    {
                                        TollName = toll.TollName,
                                        TollAmount = toll.TollAmount
                                    }));
                            }
    
                            SendTripReceipt(@event.SourceId, 
                                Convert.ToDecimal(meterAmount),
                                Convert.ToDecimal(tipAmount),
                                Convert.ToDecimal(taxAmount),
                                extra: Convert.ToDecimal(extraAmount),
                                toll: Convert.ToDecimal(tollAmount),
                                surcharge: Convert.ToDecimal(surchargeAmount),
                                cmtRideLinqFields: new SendReceipt.CmtRideLinqReceiptFields
                                {
                                    DriverId = tripInfo.DriverId.ToString(),
                                    PickUpDateTime = tripInfo.StartTime,
                                    DropOffDateTime = tripInfo.EndTime,
                                    TripId = tripInfo.TripId,
                                    Distance = tripInfo.Distance,
                                    LastFour = tripInfo.LastFour,
                                    AccessFee = accessFee,
                                    FareAtAlternateRate = fareAtAlternateRate,
                                    RateAtTripEnd = tripInfo.RateAtTripEnd,
                                    RateAtTripStart = tripInfo.RateAtTripStart,
                                    Tolls = tolls.ToArray(),
                                    TipIncentive = (order.TipIncentive.HasValue) ? order.TipIncentive.Value : 0
                                });
                        }
                    }
                } 
            }
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            @event.MigrateFees();

            if (@event.IsForPrepaidOrder || @event.FeeType == FeeTypes.Booking)
            {
                // Don't message user, he will be notified at the end of the ride
                return;
            }

            if (@event.FeeType == FeeTypes.Cancellation || @event.FeeType == FeeTypes.NoShow)
            {
                SendFeesReceipt(@event.OrderId, @event.Amount, @event.FeeType);
            }
            else
            {
                SendTripReceipt(
                    @event.OrderId, 
                    @event.Meter,
                    @event.Tip,
                    @event.Tax,
                    toll: @event.Toll,
                    surcharge: @event.Surcharge,
                    bookingFees: @event.BookingFees,
                    amountSavedByPromotion: @event.AmountSavedByPromotion);
            }
        }

        public void Handle(ManualRideLinqTripInfoUpdated @event)
        {
            var order = _orderDao.FindById(@event.SourceId);
            if (order != null)
            {
                if (@event.EndTime.HasValue)
                {
                    // ManualRideLinq order is done, send receipt

                    order.Fare = @event.Fare;
                    order.Tip = @event.Tip;
                    order.Tax = @event.Tax;
                    order.Toll = @event.Toll;
                    order.Surcharge = @event.Surcharge;
                    
                    SendTripReceipt(@event.SourceId, 
                        Convert.ToDecimal(@event.Fare ?? 0),
                        Convert.ToDecimal(@event.Tip ?? 0),
                        Convert.ToDecimal(@event.Tax ?? 0),
                        extra: Convert.ToDecimal(@event.Extra ?? 0),
                        toll: Convert.ToDecimal(@event.Toll ?? 0),
                        surcharge: Convert.ToDecimal(@event.Surcharge ?? 0),
                        cmtRideLinqFields: new SendReceipt.CmtRideLinqReceiptFields
                        {
                            DriverId = @event.DriverId.ToString(),
                            PickUpDateTime = @event.StartTime,
                            DropOffDateTime = @event.EndTime,
                            TripId = @event.TripId,
                            Distance = @event.Distance,
                            LastFour = @event.LastFour,
                            AccessFee = @event.AccessFee,
                            FareAtAlternateRate = @event.FareAtAlternateRate,
                            RateAtTripEnd = @event.RateAtTripEnd.HasValue ? Convert.ToInt32(@event.RateAtTripEnd) : 0,
                            RateAtTripStart = @event.RateAtTripStart.HasValue ? Convert.ToInt32(@event.RateAtTripStart) : 0,
                            Tolls = @event.Tolls,
                            LastLatitudeOfVehicle = @event.LastLatitudeOfVehicle,
                            LastLongitudeOfVehicle = @event.LastLongitudeOfVehicle,
                            TipIncentive = (order.TipIncentive.HasValue) ? order.TipIncentive.Value : 0
                        });
                }
            }
        }

        public void Handle(CreditCardDeactivated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                var creditCard = _creditCardDao.FindByAccountId(@event.SourceId).First();

                _notificationService.SendCreditCardDeactivatedEmail(creditCard.CreditCardCompany, creditCard.Last4Digits, account.Email, account.Language);
            }
        }

        public void Handle(UserAddedToPromotionWhiteList_V2 @event)
        {
            var promotion = _promotionDao.FindById(@event.SourceId);
            if (promotion == null)
            {
                return;
            }

            foreach (var accountId in @event.AccountIds)
            {
                var account = _accountDao.FindById(accountId);
                if (account != null)
                {
                    var accountLanguage = account.Language ?? SupportedLanguages.en.ToString();
                    _notificationService.SendPromotionUnlockedEmail(promotion.Name, promotion.Code, promotion.GetEndDateTime(), account.Email, accountLanguage);
                }
            }
        }

        private void SendFeesReceipt(Guid orderId, decimal feeAmount, FeeTypes feeType)
        {
            var order = _orderDao.FindById(orderId);
            var account = _accountDao.FindById(order.AccountId);
            var creditCard = _creditCardDao.FindByAccountId(order.AccountId).First();

            if (feeType == FeeTypes.Cancellation)
            {
                _notificationService.SendCancellationFeesReceiptEmail(order.IBSOrderId ?? 0, Convert.ToDouble(feeAmount), creditCard.Last4Digits, account.Email, account.Language);
            }
            else if (feeType == FeeTypes.NoShow)
            {
                _notificationService.SendNoShowFeesReceiptEmail(order.IBSOrderId ?? 0, Convert.ToDouble(feeAmount), order.PickupAddress, creditCard.Last4Digits, account.Email, account.Language);
            }
        }

        private void SendTripReceipt(Guid orderId, decimal meter, decimal tip, decimal tax, decimal amountSavedByPromotion = 0m,
            decimal toll = 0m, decimal extra = 0m, decimal surcharge = 0m, decimal bookingFees = 0m, SendReceipt.CmtRideLinqReceiptFields cmtRideLinqFields = null)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = _orderDao.FindById(orderId);
                var orderStatus = _orderDao.FindOrderStatusById(orderId);

                if (orderStatus != null)
                {
                    var account = context.Find<AccountDetail>(orderStatus.AccountId);
                    var orderPayment = context.Set<OrderPaymentDetail>().FirstOrDefault(p => p.OrderId == orderStatus.OrderId && p.IsCompleted );
                    
                    CreditCardDetails card = null;
                    if (orderPayment != null && orderPayment.CardToken.HasValue())
                    {
                        card = _creditCardDao.FindByToken(orderPayment.CardToken);
                    }
                    
                    // Payment was handled by app, send receipt
                    if (orderStatus.IsPrepaid && orderPayment != null)
                    {
                        // Order was prepaid, all the good amounts are in OrderPaymentDetail
                        meter = orderPayment.Meter;
                        tip = orderPayment.Tip;
                        tax = orderPayment.Tax;
                    }
                        
                    var promoUsed = _promotionDao.FindByOrderId(orderId);
                    var ibsOrderId = order.IBSOrderId;


                    decimal fare;
                    if (order.IsManualRideLinq)
                    {
                        var manualRideLinqDetail = context.Find<OrderManualRideLinqDetail>(orderStatus.OrderId);
                        ibsOrderId = manualRideLinqDetail.TripId;

                        fare = meter;
                    }
                    else
                    {
                        if (cmtRideLinqFields != null)
                        {
                            fare = meter;
                        }
                        else
                        {
                            // Meter also contains toll and surcharge, to send an accurate receipt, we need to remove both toll and surcharge.
                            fare = orderPayment.SelectOrDefault(payment => payment.Meter - payment.Toll - surcharge, meter - toll - surcharge);
                        }
                    }

                    if (cmtRideLinqFields != null && cmtRideLinqFields.DriverId.HasValue())
                    {
                        orderStatus.DriverInfos.DriverId = cmtRideLinqFields.DriverId;
                    }

                    var command = SendReceiptCommandBuilder.GetSendReceiptCommand(
                        order,
                        account,
                        ibsOrderId,
                        orderStatus.VehicleNumber,
                        orderStatus.DriverInfos,
                        Convert.ToDouble(fare),
                        Convert.ToDouble(toll),
                        Convert.ToDouble(extra),
                        Convert.ToDouble(surcharge),
                        Convert.ToDouble(bookingFees),
                        orderPayment.SelectOrDefault(payment => Convert.ToDouble(payment.Tip), Convert.ToDouble(tip)),
                        Convert.ToDouble(tax),
                        orderPayment,
                        Convert.ToDouble(amountSavedByPromotion),
                        promoUsed,
                        card,
                        cmtRideLinqFields);

                    _commandBus.Send(command);
                }
            }
        }

        private void InitializeCmtServiceClient(string companyKey, ServiceType serviceType)
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings(companyKey).CmtPaymentSettings, serviceType, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);
        }
    }
}