using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.OrderCreation;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    public class PayPalCheckoutController : BaseApiController
    {
        public PayPalCheckoutService PayPalCheckoutService { get; private set; }

        public PayPalCheckoutController(PayPalCheckoutService payPalCheckoutService)
        {
            PayPalCheckoutService = payPalCheckoutService;
        }

        [HttpGet, HttpHead, Route("api/v2/accounts/orders/{orderId}/proceed"), Auth]
        public IHttpActionResult ExecuteWebPaymentAndProceedWithOrder(ExecuteWebPaymentAndProceedWithOrder request)
        {
            var result = PayPalCheckoutService.Get(request);

            return Redirect(result);
        }
    }
}
