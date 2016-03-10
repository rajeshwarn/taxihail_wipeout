using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Maps;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using CustomerPortal.Client;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/directions")]
    public class DirectionsController : BaseApiController
    {
        public DirectionsService DirectionsService { get;  }

        public DirectionsController(IDirections client, IServerSettings serverSettings, IOrderDao orderDao, VehicleService vehicleService, ILogger logger, ICommandBus commandBus, ITaxiHailNetworkServiceClient networkServiceClient)
        {
            DirectionsService = new DirectionsService(client, serverSettings, orderDao, vehicleService, logger, commandBus, networkServiceClient);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetDirections([FromUri]DirectionsRequest request)
        {
            var result = await DirectionsService.Get(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Route("eta")]
        public IHttpActionResult GetAssignedEta([FromUri] AssignedEtaRequest request)
        {
            var result = DirectionsService.Get(request);

            return GenerateActionResult(result);
        }
    }
}
