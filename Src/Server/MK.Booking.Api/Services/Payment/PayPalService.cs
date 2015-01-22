#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PayPalService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly IOrderPaymentDao _dao;        
        private readonly PayPalServiceFactory _factory;
        private readonly IIbsOrderService _ibs;
        private readonly IAccountDao _accountDao;
        private static ILogger _logger;
        private readonly IOrderDao _orderDao;
        private readonly Resources.Resources _resources;

        public PayPalService(ICommandBus commandBus, IOrderPaymentDao dao,
            PayPalServiceFactory factory, IServerSettings serverSettings, 
            IIbsOrderService ibs, IAccountDao accountDao, ILogger logger, IOrderDao orderDao)
        {
            _commandBus = commandBus;
            _dao = dao;
            _factory = factory;
            _serverSettings = serverSettings;
            _ibs = ibs;
            _accountDao = accountDao;
            _logger = logger;
            _orderDao = orderDao;

            _resources = new Resources.Resources(serverSettings);
        }

        public PayPalExpressCheckoutPaymentResponse Post(InitiatePayPalExpressCheckoutPaymentRequest request)
        {
            // TODO
            return null;
        }

        public HttpResult Get(CancelPayPalExpressCheckoutPaymentRequest request)
        {
            var payment = _dao.FindByPayPalToken(request.Token);
            if (payment == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new CancelPayPalExpressCheckoutPayment
            {
                PaymentId = payment.PaymentId,
            });

            return new HttpResult
            {
                StatusCode = HttpStatusCode.Redirect,
                Headers =
                {
                    {HttpHeaders.Location, "taxihail://cancel"}
                }
            };
        }

        public HttpResult Get(CompletePayPalExpressCheckoutPaymentRequest request)
        {
            // TODO
            return null;
        }

        private ServerPaymentSettings GetPaymentSettings()
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();
            if (paymentSettings == null)
                throw new HttpError(HttpStatusCode.InternalServerError, "InternalServerError",
                    "Payment settings not found");

            if (paymentSettings.PayPalServerSettings == null || paymentSettings.PayPalClientSettings == null)
                throw new HttpError(HttpStatusCode.InternalServerError, "InternalServerError",
                    "PayPal settings not found");

            return paymentSettings;
        }


        public static bool TestClient(IServerSettings serverSettings, IRequestContext requestContext,
            PayPalServerCredentials payPalServerServerSettings, bool isSandbox)
        {
            try
            {
                // TODO
                //var service = new ExpressCheckoutServiceFactory(serverSettings, _logger)
                //    .CreateService(payPalServerServerSettings, isSandbox);
                //service.SetExpressCheckout(2, "http://example.net/success", "http://example.net/cancel", string.Empty);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}