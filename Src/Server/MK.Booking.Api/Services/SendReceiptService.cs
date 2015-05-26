﻿using System;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;
using CMTPayment;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using CMTPayment.Pair;
using ServiceStack.Common.Utils;

namespace apcurium.MK.Booking.Api.Services
{
    public class SendReceiptService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IPromotionDao _promotionDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly ICommandBus _commandBus;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;

        public SendReceiptService(
            ICommandBus commandBus,
            IIBSServiceProvider ibsServiceProvider,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IPromotionDao promotionDao,
            IServerSettings serverSettings,
            ILogger logger)
        {
            _serverSettings = serverSettings;
            _logger = logger;
            _ibsServiceProvider = ibsServiceProvider;
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _accountDao = accountDao;
            _promotionDao = promotionDao;
            _creditCardDao = creditCardDao;
            _commandBus = commandBus;
        }

        public object Post(SendReceipt request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (!order.IBSOrderId.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.OrderNotInIbs.ToString());
            }

            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Not your order");
            }

            // If the order was created in another company, need to fetch the correct IBS account
            var ibsAccountId = _accountDao.GetIbsAccountId(account.Id, order.CompanyKey);

            if (!ibsAccountId.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.IBSAccountNotFound.ToString());
            }

            var ibsOrder = _ibsServiceProvider.Booking(order.CompanyKey).GetOrderDetails(order.IBSOrderId.Value, ibsAccountId.Value, order.Settings.Phone);

            var orderPayment = _orderPaymentDao.FindByOrderId(order.Id);
            var pairingInfo = _orderDao.FindOrderPairingById(order.Id);
            var orderStatus = _orderDao.FindOrderStatusById(request.OrderId);

            double? meterAmount;
            double? tollAmount = null;
            double? tipAmount;
            double? taxAmount;
            double? surcharge;
            double? extraAmount = null;
            PromotionUsageDetail promotionUsed = null;
            ReadModel.CreditCardDetails creditCard = null;

            var ibsOrderId = orderStatus.IBSOrderId;

            if (orderPayment != null && orderPayment.IsCompleted)
            {
                meterAmount = Convert.ToDouble(orderPayment.Meter);
                tipAmount = Convert.ToDouble(orderPayment.Tip);
                taxAmount = Convert.ToDouble(orderPayment.Tax);
                surcharge = Convert.ToDouble(orderPayment.Surcharge);
                
                // promotion can only be used with in app payment
                promotionUsed = _promotionDao.FindByOrderId(request.OrderId);

                creditCard = orderPayment.CardToken.HasValue()
                    ? _creditCardDao.FindByToken(orderPayment.CardToken)
                    : null;
            }
            else if (pairingInfo != null && pairingInfo.AutoTipPercentage.HasValue)
            {
                var tripInfo = GetTripInfo(pairingInfo.PairingToken);
                if (tripInfo != null && tripInfo.EndTime.HasValue)
                {
                    // this is for CMT RideLinq only, no VAT

                    var tollHistory = tripInfo.TollHistory != null
								? tripInfo.TollHistory.Sum(p => p.TollAmount)
								: 0;;

                    meterAmount = Math.Round(((double)tripInfo.Fare / 100), 2);
					tollAmount = Math.Round(((double)tollHistory / 100), 2);
                    extraAmount = Math.Round(((double) tripInfo.Extra/100), 2);
                    tipAmount = Math.Round(((double)tripInfo.Tip / 100), 2);
                    taxAmount = Math.Round(((double)tripInfo.Tax / 100), 2);
                    surcharge = Math.Round(((double) tripInfo.Tax/100), 2);
                    orderStatus.DriverInfos.DriverId = tripInfo.DriverId.ToString();
                    ibsOrderId = tripInfo.TripId;
                }
                else
                {
                    meterAmount = ibsOrder.Fare;
                    tollAmount = ibsOrder.Toll;
                    tipAmount = FareHelper.CalculateTipAmount(ibsOrder.Fare.GetValueOrDefault(0), pairingInfo.AutoTipPercentage.Value);
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
                meterAmount = ibsOrder.Fare;
                tollAmount = ibsOrder.Toll;
                tipAmount = ibsOrder.Tip;
                taxAmount = ibsOrder.VAT;
                surcharge = order.Surcharge;

                orderPayment = null;
            }

            _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(
                    order, 
                    account, 
                    ibsOrderId, 
                    ibsOrder.VehicleNumber, 
                    orderStatus.DriverInfos,
                    meterAmount,
                    tollAmount,
                    extraAmount,
                    surcharge,
                    tipAmount,
                    taxAmount,
                    orderPayment,
                    promotionUsed != null
                        ? Convert.ToDouble(promotionUsed.AmountSaved)
                        : (double?)null,
                    promotionUsed,
                    creditCard));

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        private Trip GetTripInfo(string pairingToken)
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            var cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);

            return cmtTripInfoServiceHelper.GetTripInfo(pairingToken);
        }
    }
}