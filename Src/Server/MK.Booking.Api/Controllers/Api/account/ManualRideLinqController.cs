using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    public class ManualRideLinqController : BaseApiController
    {
        public ManualRidelinqOrderService ManualRidelinqOrderService { get; private set; }

        public ManualRideLinqController(ManualRidelinqOrderService manualRidelinqOrderService)
        {
            ManualRidelinqOrderService = manualRidelinqOrderService;
        }

        [HttpGet, Route("api/accounts/manualridelinq/{orderId}"), Auth]
        public IHttpActionResult GetManualRideling(Guid orderId)
        {
            var result = ManualRidelinqOrderService.Get(new ManualRideLinqRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("api/accounts/manualridelinq/{orderId}"), Auth]
        public async Task<IHttpActionResult> DeleteManualRideling(Guid orderId)
        {
            await ManualRidelinqOrderService.Delete(new ManualRideLinqRequest() { OrderId = orderId });

            return Ok();
        }

        [HttpPost, Auth, Route("api/accounts/manualridelinq")]
        public async Task<IHttpActionResult> PairWithManualRidelinq([FromBody] ManualRideLinqPairingRequest request)
        {
            var result = await ManualRidelinqOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/accounts/manualridelinq/{orderId}/tip"), Auth]
        public async Task<IHttpActionResult> UpdateAutoTip(Guid orderId, [FromBody] ManualRideLinqUpdateAutoTipRequest request)
        {
            request.OrderId = orderId;

            var result = await ManualRidelinqOrderService.Put(request);

            return GenerateActionResult(result);
        }
    }
}
