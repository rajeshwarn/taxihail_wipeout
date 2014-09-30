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
        private readonly IServerSettings _appSettings;

        public SendReceiptService(
            ICommandBus commandBus,
            IBookingWebServiceClient bookingWebServiceClient,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IConfigurationManager configurationManager,
            IServerSettings appSettings
            )
        {
            _configurationManager = configurationManager;
            _appSettings = appSettings;
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
            
            if ((orderPayment != null) && (orderPayment.IsCompleted))
            {
                var creditCard = orderPayment.CardToken.HasValue() 
                    ? _creditCardDao.FindByToken(orderPayment.CardToken) 
                    : null;

                _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(
                    order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos.FullName,
                    GetMeterAmount((double)orderPayment.Meter),
                    0, 
                    Convert.ToDouble(orderPayment.Tip),
                    GetTaxAmount((double)orderPayment.Meter, 0), 
                    orderPayment, creditCard));
            }
            else if ((pairingInfo != null) && (pairingInfo.AutoTipPercentage.HasValue))
            {
                var creditCard = pairingInfo.TokenOfCardToBeUsedForPayment.HasValue() 
                    ? _creditCardDao.FindByToken(pairingInfo.TokenOfCardToBeUsedForPayment) 
                    : null;
                var tripData = GetTripData(pairingInfo.PairingToken);
                if ((tripData != null) && (tripData.EndTime.HasValue))
                {
                    // this is for CMT RideLinq only, no VAT
                    _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(
                        order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos.FullName,
                        Math.Round(((double)tripData.Fare / 100), 2), 
                        Math.Round(((double)tripData.Extra / 2), 2), 
                        Math.Round(((double)tripData.Tip / 100), 2), 
                        Math.Round(((double)tripData.Tax / 100), 2), 
                        null, creditCard));
                }
                else
                {
                    _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(
                        order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos.FullName,
                        GetMeterAmount(ibsOrder.Fare),
                        ibsOrder.Toll,
                        GetTipAmount(ibsOrder.Fare.GetValueOrDefault(0), pairingInfo.AutoTipPercentage.Value),
                        GetTaxAmount(ibsOrder.Fare, ibsOrder.VAT), 
                        null, creditCard));
                }
            }
            else
            {
                _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(
                    order, account, ibsOrder.VehicleNumber, orderStatus.DriverInfos.FullName,
                    GetMeterAmount(ibsOrder.Fare), 
                    ibsOrder.Toll, 
                    ibsOrder.Tip, 
                    GetTaxAmount(ibsOrder.Fare, ibsOrder.VAT), 
                    null, null));
            }

            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        private double GetMeterAmount(double? meterAmount)
        {
            if (!meterAmount.HasValue)
            {
                return 0;
            }
            // if VAT is enabled, we need to substract the VAT amount from the meter amount, otherwise we return it as is
            return _appSettings.Data.VATIsEnabled
                ? Fare.FromAmountInclTax(meterAmount.Value, _appSettings.Data.VATPercentage).AmountExclTax
                : Fare.FromAmountInclTax(meterAmount.Value, 0).AmountExclTax;
        }

        private double GetTaxAmount(double? meterAmount, double? taxAmountIfVATIsDisabled)
        {
            return _appSettings.Data.VATIsEnabled
                ? Fare.FromAmountInclTax(meterAmount.GetValueOrDefault(0), _appSettings.Data.VATPercentage).TaxAmount
                : taxAmountIfVATIsDisabled.GetValueOrDefault(0);
        }

        private double GetTipAmount(double amount, double percentage)
        {
            var tip = percentage / 100;
            return Math.Round(amount * tip, 2);
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