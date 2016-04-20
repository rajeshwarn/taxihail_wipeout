using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;

namespace apcurium.MK.Web.Controllers.Api
{
    public class CmtPaymentPairingController : BaseApiController
    {
        public CmtPaymentPairingService CmtPaymentService { get; private set; }

        public CmtPaymentPairingController(CmtPaymentPairingService cmtPaymentService)
        {
            CmtPaymentService = cmtPaymentService;
        }

        [HttpPost, Route("api/order/pairing")]
        public async Task<IHttpActionResult> Pair(CmtPaymentPairingRequest request)
        {
            var result = await CmtPaymentService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
