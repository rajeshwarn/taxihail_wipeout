using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/payments/braintree")]
    public class BraintreeClientPaymentController : BaseApiController
    {
        public BraintreeClientPaymentService BraintreeClientPaymentService { get; }

        public BraintreeClientPaymentController(IServerSettings serverSettings)
        {
            BraintreeClientPaymentService = new BraintreeClientPaymentService(serverSettings);
        }

        [HttpPost, Auth, Route("tokenize")]
        public IHttpActionResult TokenizeCreditCard(TokenizeCreditCardBraintreeRequest request)
        {
            var result = BraintreeClientPaymentService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Auth, Route("generateclienttoken")]
        public IHttpActionResult GetClientToken()
        {
            var result = BraintreeClientPaymentService.Get();

            return GenerateActionResult(result);
        }
    }
}
