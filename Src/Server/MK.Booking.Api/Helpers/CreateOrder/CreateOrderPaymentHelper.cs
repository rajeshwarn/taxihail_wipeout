using System;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;

namespace apcurium.MK.Booking.Api.Helpers.CreateOrder
{
    public class CreateOrderPaymentHelper
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly IPaymentService _paymentService;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IPayPalServiceFactory _payPalServiceFactory;

        public CreateOrderPaymentHelper(
            IServerSettings serverSettings,
            ICommandBus commandBus,
            IPaymentService paymentService,
            IOrderPaymentDao orderPaymentDao,
            IPayPalServiceFactory payPalServiceFactory)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _paymentService = paymentService;
            _orderPaymentDao = orderPaymentDao;
            _payPalServiceFactory = payPalServiceFactory;
        }

        internal bool PreAuthorizePaymentMethod(
            string companyKey,
            Guid orderId,
            AccountDetail account,
            string clientLanguageCode,
            bool isFutureBooking,
            decimal? appEstimateWithTip,
            decimal bookingFees,
            bool isPayPal,
            CreateReportOrder createReportOrder,
            string cvv = null)
        {
            if (!_serverSettings.GetPaymentSettings(companyKey).IsPreAuthEnabled || isFutureBooking)
            {
                // preauth will be done later, save the info temporarily
                if (_serverSettings.GetPaymentSettings().AskForCVVAtBooking)
                {
                    _commandBus.Send(new SaveTemporaryOrderPaymentInfo { OrderId = orderId, Cvv = cvv });
                }

                return true;
            }

            // there's a minimum amount of $50 (warning indicating that on the admin ui)
            // if app returned an estimate, use it, otherwise use the setting (or 0), then use max between the value and 50
            if (appEstimateWithTip.HasValue)
            {
                appEstimateWithTip = appEstimateWithTip.Value + bookingFees;
            }

            var preAuthAmount = Math.Max(appEstimateWithTip ?? (_serverSettings.GetPaymentSettings(companyKey).PreAuthAmount ?? 0), 50);

            var preAuthResponse = _paymentService.PreAuthorize(companyKey, orderId, account, preAuthAmount, cvv: cvv);

            return preAuthResponse.IsSuccessful;
        }

        internal BasePaymentResponse CapturePaymentForPrepaidOrder(
            string companyKey,
            Guid orderId,
            AccountDetail account,
            decimal appEstimateWithTip,
            int tipPercentage,
            decimal bookingFees,
            string cvv,
            CreateReportOrder createReportOrder)
        {
            // Note: No promotion on web
            var tipAmount = FareHelper.GetTipAmountFromTotalIncludingTip(appEstimateWithTip, tipPercentage);
            var totalAmount = appEstimateWithTip + bookingFees;
            var meterAmount = appEstimateWithTip - tipAmount;

            var preAuthResponse = _paymentService.PreAuthorize(companyKey, orderId, account, totalAmount, isForPrepaid: true, cvv: cvv);
            if (preAuthResponse.IsSuccessful)
            {
                // Wait for payment to be created
                Thread.Sleep(500);

                var commitResponse = _paymentService.CommitPayment(
                    companyKey,
                    orderId,
                    account,
                    totalAmount,
                    totalAmount,
                    meterAmount,
                    tipAmount,
                    preAuthResponse.TransactionId,
                    preAuthResponse.ReAuthOrderId,
                    isForPrepaid: true);

                if (commitResponse.IsSuccessful)
                {
                    var paymentDetail = _orderPaymentDao.FindByOrderId(orderId, companyKey);

                    var fareObject = FareHelper.GetFareFromAmountInclTax(meterAmount,
                        _serverSettings.ServerData.VATIsEnabled
                            ? _serverSettings.ServerData.VATPercentage
                            : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        AccountId = account.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(companyKey, orderId),
                        TotalAmount = totalAmount,
                        MeterAmount = fareObject.AmountExclTax,
                        TipAmount = tipAmount,
                        TaxAmount = fareObject.TaxAmount,
                        AuthorizationCode = commitResponse.AuthorizationCode,
                        TransactionId = commitResponse.TransactionId,
                        IsForPrepaidOrder = true,
                        BookingFees = bookingFees
                    });
                }
                else
                {
                    // Payment failed, void preauth
                    _paymentService.VoidPreAuthorization(companyKey, orderId, true);

                    return new BasePaymentResponse
                    {
                        IsSuccessful = false,
                        Message = commitResponse.Message
                    };
                }
            }
            else
            {
                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = preAuthResponse.Message
                };
            }

            return new BasePaymentResponse { IsSuccessful = true };
        }

        internal InitializePayPalCheckoutResponse InitializePayPalCheckoutIfNecessary(
            Guid accountId, bool isPrepaid, Guid orderId, Contract.Requests.CreateOrder request,
            decimal bookingFees, string companyKey, CreateReportOrder createReportOrder, string absoluteRequestUri)
        {
            if (isPrepaid
                && request.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
            {
                var paypalWebPaymentResponse = _payPalServiceFactory.GetInstance(companyKey).InitializeWebPayment(accountId, orderId, absoluteRequestUri, request.Estimate.Price, bookingFees, request.ClientLanguageCode);

                if (paypalWebPaymentResponse.IsSuccessful)
                {
                    return paypalWebPaymentResponse;
                }

                var createOrderException = new HttpError(HttpStatusCode.BadRequest, ErrorCode.CreateOrder_RuleDisable.ToString(), paypalWebPaymentResponse.Message);

                createReportOrder.Error = createOrderException.ToString();
                _commandBus.Send(createReportOrder);
                throw createOrderException;
            }

            return null;
        }
    }
}
