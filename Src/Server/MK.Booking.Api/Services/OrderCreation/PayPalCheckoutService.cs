using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Api.Services.OrderCreation
{
    public class PayPalCheckoutService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly IPayPalServiceFactory _payPalServiceFactory;
        private readonly IServerSettings _serverSettings;
        private readonly Resources.Resources _resources;

        public PayPalCheckoutService(
            ICommandBus commandBus,
            ILogger logger,
            IOrderDao orderDao,
            IAccountDao accountDao,
            IPayPalServiceFactory payPalServiceFactory,
            IServerSettings serverSettings)
        {
            _commandBus = commandBus;
            _logger = logger;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _payPalServiceFactory = payPalServiceFactory;
            _serverSettings = serverSettings;
            _resources = new Resources.Resources(_serverSettings);
        }

        public object Get(ExecuteWebPaymentAndProceedWithOrder request)
        {
            _logger.LogMessage("ExecuteWebPaymentAndProceedWithOrder request : " + request.ToJson());

            var temporaryInfo = _orderDao.GetTemporaryInfo(request.OrderId);
            var orderInfo = JsonSerializer.DeserializeFromString<TemporaryOrderCreationInfo>(temporaryInfo.SerializedOrderCreationInfo);

            if (request.Cancel || orderInfo == null)
            {
                var clientLanguageCode = orderInfo == null
                    ? SupportedLanguages.en.ToString()
                    : orderInfo.Request.ClientLanguageCode;

                _commandBus.Send(new CancelOrderBecauseOfError
                {
                    OrderId = request.OrderId,
                    ErrorDescription = _resources.Get("CannotCreateOrder_PrepaidPayPalPaymentCancelled", clientLanguageCode)
                });
            }
            else
            {
                // Execute PayPal payment
                var response = _payPalServiceFactory.GetInstance(orderInfo.BestAvailableCompany.CompanyKey).ExecuteWebPayment(request.PayerId, request.PaymentId);

                if (response.IsSuccessful)
                {
                    var account = _accountDao.FindById(orderInfo.AccountId);

                    var tipPercentage = account.DefaultTipPercent ?? _serverSettings.ServerData.DefaultTipPercentage;
                    var tipAmount = FareHelper.CalculateTipAmount(orderInfo.Request.Fare.AmountInclTax, tipPercentage);

                    _commandBus.Send(new MarkPrepaidOrderAsSuccessful
                    {
                        OrderId = request.OrderId,
                        TotalAmount = orderInfo.Request.Fare.AmountInclTax + tipAmount,
                        MeterAmount = orderInfo.Request.Fare.AmountExclTax,
                        TaxAmount = orderInfo.Request.Fare.TaxAmount,
                        TipAmount = tipAmount,
                        TransactionId = response.TransactionId,
                        Provider = PaymentProvider.PayPal,
                        Type = PaymentType.PayPal
                    });
                }
                else
                {
                    _commandBus.Send(new CancelOrderBecauseOfError
                    {
                        OrderId = request.OrderId,
                        ErrorDescription = response.Message
                    });
                }
            }

            // Build url used to redirect the web client to the booking status view
            string baseUrl;

            if (_serverSettings.ServerData.BaseUrl.HasValue())
            {
                baseUrl = _serverSettings.ServerData.BaseUrl;
            }
            else
            {
                baseUrl = Request.AbsoluteUri
                    .Replace(Request.PathInfo, string.Empty)
                    .Replace(GetAppHost().Config.ServiceStackHandlerFactoryPath, string.Empty)
                    .Replace(Request.QueryString.ToString(), string.Empty)
                    .Replace("?", string.Empty);
            }

            var redirectUrl = string.Format("{0}#status/{1}", baseUrl, request.OrderId);

            return new HttpResult
            {
                StatusCode = HttpStatusCode.Redirect,
                Headers = { { HttpHeaders.Location, redirectUrl } }
            };
        }
    }
}
