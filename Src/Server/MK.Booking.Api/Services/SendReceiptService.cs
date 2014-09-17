#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Pair;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
using apcurium.MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class SendReceiptService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly ICommandBus _commandBus;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IConfigurationManager _configurationManager;

        public SendReceiptService(
            ICommandBus commandBus,
            IBookingWebServiceClient bookingWebServiceClient,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IConfigurationManager configurationManager
            )
        {
            _configurationManager = configurationManager;
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _accountDao = accountDao;
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

            var ibsOrder = _bookingWebServiceClient.GetOrderDetails(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);

            var orderPayment = _orderPaymentDao.FindByOrderId(order.Id);
            var pairingInfo = _orderDao.FindOrderPairingById(order.Id);
            var orderStatus = _orderDao.FindOrderStatusById(request.OrderId);
            var vat = ibsOrder.VAT;

            var vatEnabled = _configurationManager.GetSetting("VATIsEnabled", false);
            if (vatEnabled)
            {
                // Must manually calculate VAT since it's included in the price received from IBS
                var taxPercentage = _configurationManager.GetSetting("VATPercentage", 0d);
                if (ibsOrder.Fare != null)
                {
                    vat = Fare.FromAmountInclTax(ibsOrder.Fare.Value,taxPercentage).TaxAmount;
                }
            }

            if ((orderPayment != null) && (orderPayment.IsCompleted))
            {
                var creditCard = orderPayment.CardToken.HasValue() ? _creditCardDao.FindByToken(orderPayment.CardToken) : null;
                _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos.FullName,
                    Convert.ToDouble(orderPayment.Meter), 0, Convert.ToDouble(orderPayment.Tip), 0, orderPayment, creditCard));
            }
            else if ((pairingInfo != null) && (pairingInfo.AutoTipPercentage.HasValue))
            {
                var creditCard = pairingInfo.TokenOfCardToBeUsedForPayment.HasValue() ? _creditCardDao.FindByToken(pairingInfo.TokenOfCardToBeUsedForPayment) : null;
                var tripData = GetTripData(pairingInfo.PairingToken);
                if ((tripData != null) && (tripData.EndTime.HasValue))
                {
                    _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos.FullName,
                        Math.Round(((double)tripData.Fare / 100), 2), Math.Round(((double)tripData.Extra / 2), 2), Math.Round(((double)tripData.Tip / 100), 2), Math.Round(((double)tripData.Tax / 100), 2), null, creditCard));
                }
                else
                {

                    _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos.FullName,
                        ibsOrder.Fare, ibsOrder.Toll, Math.Round(((double)pairingInfo.AutoTipPercentage.Value) / 100, 2), vat, null, creditCard));
                }
            }
            else
            {
                _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos.FullName, ibsOrder.Fare, ibsOrder.Toll, ibsOrder.Tip, vat, null, null));
            }

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        private Trip GetTripData(string pairingToken)
        {
            try
            {
                var cmtClient = new CmtMobileServiceClient(_configurationManager.GetPaymentSettings().CmtPaymentSettings, null, null);
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