using System;
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
using CMTPayment.Pair;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Configuration;

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

        public SendReceiptService(
            ICommandBus commandBus,
            IIBSServiceProvider ibsServiceProvider,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IPromotionDao promotionDao,
            IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
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

            // If the order was created in another market, need to fetch the correct IBS account
            var ibsAccountId = order.Market.HasValue()
                ? _accountDao.GetIbsAccountId(account.Id, order.CompanyKey).Value
                : account.IBSAccountId.Value;

            var ibsOrder = _ibsServiceProvider.Booking(order.CompanyKey).GetOrderDetails(order.IBSOrderId.Value, ibsAccountId, order.Settings.Phone);

            var orderPayment = _orderPaymentDao.FindByOrderId(order.Id);
            var pairingInfo = _orderDao.FindOrderPairingById(order.Id);
            var orderStatus = _orderDao.FindOrderStatusById(request.OrderId);

            double? meterAmount;
            double? tollAmount;
            double? tipAmount;
            double? taxAmount;
            PromotionUsageDetail promotionUsed = null;
            ReadModel.CreditCardDetails creditCard = null;

            if (orderPayment != null && orderPayment.IsCompleted)
            {
                meterAmount = Convert.ToDouble(orderPayment.Meter);
                tollAmount = 0;
                tipAmount = Convert.ToDouble(orderPayment.Tip);
                taxAmount = Convert.ToDouble(orderPayment.Tax);

                // promotion can only be used with in app payment
                promotionUsed = _promotionDao.FindByOrderId(request.OrderId);

                creditCard = orderPayment.CardToken.HasValue()
                    ? _creditCardDao.FindByToken(orderPayment.CardToken)
                    : null;
            }
            else if (pairingInfo != null && pairingInfo.AutoTipPercentage.HasValue)
            {
                var tripData = GetTripData(pairingInfo.PairingToken);
                if (tripData != null && tripData.EndTime.HasValue)
                {
                    // this is for CMT RideLinq only, no VAT

                    meterAmount = Math.Round(((double) tripData.Fare/100), 2);
                    tollAmount = Math.Round(((double) tripData.Extra/2), 2);
                    tipAmount = Math.Round(((double) tripData.Tip/100), 2);
                    taxAmount = Math.Round(((double) tripData.Tax/100), 2);
                }
                else
                {
                    meterAmount = ibsOrder.Fare;
                    tollAmount = ibsOrder.Toll;
                    tipAmount = FareHelper.CalculateTipAmount(ibsOrder.Fare.GetValueOrDefault(0), pairingInfo.AutoTipPercentage.Value);
                    taxAmount = ibsOrder.VAT;
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

                orderPayment = null;
            }

            _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos,
                    meterAmount,
                    tollAmount,
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

        private Trip GetTripData(string pairingToken)
        {
            try
            {
                var cmtClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
                var trip = cmtClient.Get(new TripRequest { Token = pairingToken });

                return trip;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}