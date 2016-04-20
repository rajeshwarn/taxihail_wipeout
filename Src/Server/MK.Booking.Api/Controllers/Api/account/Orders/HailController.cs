using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Client;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.OrderCreation;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    [Auth]
    public class HailController : BaseApiController
    {
        public HailService HailService { get; private set; }

        public HailController(HailService hailService)
        {
            HailService = hailService;
        }

        [HttpPost, Route("api/client/hail")]
        public async Task<IHttpActionResult> Hail([FromBody]HailRequest request)
        {
            var result = await HailService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/client/hail/confirm")]
        public IHttpActionResult ConfirmHail([FromBody]ConfirmHailRequest request)
        {
            var result = HailService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
