﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;
using CMTPayment;
using Infrastructure.Messaging;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using CMTPayment.Pair;

namespace apcurium.MK.Booking.Api.Services
{
    public class SendReceiptService : BaseApiService
    {
        private readonly IAccountDao _accountDao;
        private readonly IPromotionDao _promotionDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly ICommandBus _commandBus;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IReportDao _reportDao;
        private readonly IServerSettings _serverSettings;
        private readonly IGeocoding _geocoding;
        private readonly ILogger _logger;

        public SendReceiptService(
            ICommandBus commandBus,
            IIBSServiceProvider ibsServiceProvider,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IPromotionDao promotionDao,
            IReportDao reportDao,
            IServerSettings serverSettings,
            IGeocoding geocoding,
            ILogger logger)
        {
            _serverSettings = serverSettings;
            _logger = logger;
            _ibsServiceProvider = ibsServiceProvider;
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _accountDao = accountDao;
            _promotionDao = promotionDao;
            _reportDao = reportDao;
            _creditCardDao = creditCardDao;
            _geocoding = geocoding;
            _commandBus = commandBus;
        }
        
        public async Task Post(Guid orderId, string recipientEmail)
        {
            var order = _orderDao.FindById(orderId);
            if (order == null || !order.IBSOrderId.HasValue)
            {
                throw GenerateException(HttpStatusCode.BadRequest, ErrorCode.OrderNotInIbs.ToString());
            }

            AccountDetail account;
            // if the admin is requesting the receipt then it won't be for the logged in user
            if (recipientEmail.IsNullOrEmpty())
            {
                account = _accountDao.FindById(order.AccountId);
            }
            else
            {
                account = _accountDao.FindById(Session.UserId);
                if (account.Id != order.AccountId)
                {
                    throw GenerateException(HttpStatusCode.Unauthorized, "Not your order");
                }
            }

            // If the order was created in another company, need to fetch the correct IBS account
            var ibsAccountId = _accountDao.GetIbsAccountId(account.Id, order.CompanyKey);

            if (!ibsAccountId.HasValue)
            {
                throw GenerateException(HttpStatusCode.BadRequest, ErrorCode.IBSAccountNotFound.ToString());
            }

            var ibsOrder = _ibsServiceProvider.Booking(order.CompanyKey).GetOrderDetails(order.IBSOrderId.Value, ibsAccountId.Value, order.Settings.Phone);

            var orderPayment = _orderPaymentDao.FindByOrderId(order.Id, order.CompanyKey);
            var pairingInfo = _orderDao.FindOrderPairingById(order.Id);
            var orderStatus = _orderDao.FindOrderStatusById(orderId);

            double? fareAmount;
            double? tollAmount = null;
            double? tipAmount;
            double? taxAmount;
            double? surcharge;
            double? bookingFees = null;
            double? extraAmount = null;
            PromotionUsageDetail promotionUsed = null;
            ReadModel.CreditCardDetails creditCard = null;

            var ibsOrderId = orderStatus.IBSOrderId;
            Commands.SendReceipt.CmtRideLinqReceiptFields cmtRideLinqFields = null;

            if (orderPayment != null && orderPayment.IsCompleted)
            {
                fareAmount = Convert.ToDouble(orderPayment.Meter);
                tipAmount = Convert.ToDouble(orderPayment.Tip);
                taxAmount = Convert.ToDouble(orderPayment.Tax);
                surcharge = Convert.ToDouble(orderPayment.Surcharge);
                bookingFees = Convert.ToDouble(orderPayment.BookingFees);

                // promotion can only be used with in app payment
                promotionUsed = _promotionDao.FindByOrderId(orderId);

                creditCard = orderPayment.CardToken.HasValue()
                    ? _creditCardDao.FindByToken(orderPayment.CardToken)
                    : null;
            }
            else if (pairingInfo == null && order.IsManualRideLinq)
            {
                var manualRideLinqDetail = _orderDao.GetManualRideLinqById(order.Id);
                fareAmount = manualRideLinqDetail.Fare;
                ibsOrderId = manualRideLinqDetail.TripId;
                tollAmount = manualRideLinqDetail.Toll;
                extraAmount = manualRideLinqDetail.Extra;
                tipAmount = manualRideLinqDetail.Tip;
                taxAmount = manualRideLinqDetail.Tax;
                surcharge = manualRideLinqDetail.Surcharge;
                orderStatus.DriverInfos.DriverId = manualRideLinqDetail.DriverId.ToString();
                order.DropOffAddress = _geocoding.TryToGetExactDropOffAddress(orderStatus, manualRideLinqDetail.LastLatitudeOfVehicle, manualRideLinqDetail.LastLongitudeOfVehicle, order.DropOffAddress, order.ClientLanguageCode);
                
                cmtRideLinqFields = new Commands.SendReceipt.CmtRideLinqReceiptFields
                {
                    TripId = manualRideLinqDetail.TripId,
                    DriverId = manualRideLinqDetail.DriverId.ToString(),
                    Distance = manualRideLinqDetail.Distance,
                    AccessFee = manualRideLinqDetail.AccessFee,
                    PickUpDateTime = manualRideLinqDetail.StartTime,
                    DropOffDateTime = manualRideLinqDetail.EndTime,
                    LastFour = manualRideLinqDetail.LastFour,
                    FareAtAlternateRate = manualRideLinqDetail.FareAtAlternateRate,
                    RateAtTripEnd = (int)(manualRideLinqDetail.RateAtTripEnd.GetValueOrDefault()),
                    RateAtTripStart = (int)(manualRideLinqDetail.RateAtTripStart.GetValueOrDefault()),
                    LastLatitudeOfVehicle = order.DropOffAddress.Latitude,
                    LastLongitudeOfVehicle = order.DropOffAddress.Longitude,
                    TipIncentive = order.TipIncentive ?? 0
                };


            }
            else if (pairingInfo != null && pairingInfo.AutoTipPercentage.HasValue)
            {
                var tripInfo = await GetTripInfo(pairingInfo.PairingToken);
                if (tripInfo != null && !tripInfo.ErrorCode.HasValue && tripInfo.EndTime.HasValue)
                {
                    // this is for CMT RideLinq only, no VAT

                    fareAmount = Math.Round(((double)tripInfo.Fare / 100), 2);
                    var tollHistory = tripInfo.TollHistory != null
                        ? tripInfo.TollHistory.Sum(p => p.TollAmount)
                        : 0;

                    tollAmount = Math.Round(((double)tollHistory / 100), 2);
                    extraAmount = Math.Round(((double)tripInfo.Extra / 100), 2);
                    tipAmount = Math.Round(((double)tripInfo.Tip / 100), 2);
                    taxAmount = Math.Round(((double)tripInfo.Tax / 100), 2);
                    surcharge = Math.Round(((double)tripInfo.Surcharge / 100), 2);
                    orderStatus.DriverInfos.DriverId = tripInfo.DriverId.ToString();

                    cmtRideLinqFields = new Commands.SendReceipt.CmtRideLinqReceiptFields
                    {
                        TripId = tripInfo.TripId,
                        DriverId = tripInfo.DriverId.ToString(),
                        Distance = tripInfo.Distance,
                        AccessFee = Math.Round(((double)tripInfo.AccessFee / 100), 2),
                        PickUpDateTime = tripInfo.StartTime,
                        DropOffDateTime = tripInfo.EndTime,
                        LastFour = tripInfo.LastFour,
                        FareAtAlternateRate = Math.Round(((double)tripInfo.FareAtAlternateRate / 100), 2),
                        RateAtTripEnd = tripInfo.RateAtTripEnd,
                        RateAtTripStart = tripInfo.RateAtTripStart,
                        Tolls = tripInfo.TollHistory,
                        TipIncentive = order.TipIncentive ?? 0
                    };
                }
                else
                {
                    fareAmount = ibsOrder.Fare;
                    tollAmount = ibsOrder.Toll;
                    tipAmount = FareHelper.CalculateTipAmount(ibsOrder.Fare.GetValueOrDefault(0),
                        pairingInfo.AutoTipPercentage.Value);
                    taxAmount = ibsOrder.VAT;
                    surcharge = order.Surcharge;
                }

                orderPayment = null;
                creditCard = pairingInfo.TokenOfCardToBeUsedForPayment.HasValue()
                    ? _creditCardDao.FindByToken(pairingInfo.TokenOfCardToBeUsedForPayment)
                    : null;
            }
            else
            {
                fareAmount = ibsOrder.Fare;
                tollAmount = ibsOrder.Toll;
                tipAmount = ibsOrder.Tip;
                taxAmount = ibsOrder.VAT;
                surcharge = order.Surcharge;

                orderPayment = null;
            }

            var orderReport = _reportDao.GetOrderReportWithOrderId(order.Id);

            var sendReceiptCommand = SendReceiptCommandBuilder.GetSendReceiptCommand(
                    order,
                    account,
                    ibsOrderId,
                    (orderReport != null ? orderReport.VehicleInfos.Number : ibsOrder.VehicleNumber),
                    orderStatus.DriverInfos,
                    fareAmount,
                    tollAmount,
                    extraAmount,
                    surcharge,
                    bookingFees,
                    tipAmount,
                    taxAmount,
                    orderPayment,
                    promotionUsed != null
                        ? Convert.ToDouble(promotionUsed.AmountSaved)
                        : (double?)null,
                    promotionUsed,
                    creditCard,
                    cmtRideLinqFields);
            if (!recipientEmail.IsNullOrEmpty())
            {
                sendReceiptCommand.EmailAddress = recipientEmail;
            }
            _commandBus.Send(sendReceiptCommand);
        }

        private Task<Trip> GetTripInfo(string pairingToken)
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null, null);
            var cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);

            return cmtTripInfoServiceHelper.GetTripInfo(pairingToken);
        }
    }
}