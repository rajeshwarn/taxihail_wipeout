using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.OrderCreation;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    [RoutePrefix("api/v2/account/orders")]
    public class PayPalCheckoutController : BaseApiController
    {
        public PayPalCheckoutService PayPalCheckoutService { get; }

        public PayPalCheckoutController(ICommandBus commandBus,
            ILogger logger,
            IOrderDao orderDao,
            IAccountDao accountDao,
            IPayPalServiceFactory payPalServiceFactory,
            IServerSettings serverSettings)
        {
            PayPalCheckoutService = new PayPalCheckoutService(commandBus, logger, orderDao, accountDao, payPalServiceFactory, serverSettings);
        }

        [HttpGet, Route("{orderId}/proceed"), Auth]
        public IHttpActionResult ExecuteWebPaymentAndProceedWithOrder(ExecuteWebPaymentAndProceedWithOrder request)
        {
            var result = PayPalCheckoutService.Get(request);

            return Redirect(result);
        }
    }
}
