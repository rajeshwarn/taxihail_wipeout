using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    public class OverduePaymentsController : BaseApiController
    {
        public OverduePaymentService OverduePaymentService { get; private set; }


        public OverduePaymentsController(OverduePaymentService overduePaymentService)
        {
            OverduePaymentService = overduePaymentService;
        }

        [HttpGet, Auth, Route("~/api/v2/accounts/overduepayment")]
        public IHttpActionResult GetOverduePayment()
        {
            var result = OverduePaymentService.Get();

            return GenerateActionResult(result);
        }

        [Route("~/api/v2/accounts/settleoverduepayment"), HttpPost, Auth]
        public IHttpActionResult SettleOverduePayment([FromBody]SettleOverduePaymentRequest request)
        {
            var result = OverduePaymentService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
