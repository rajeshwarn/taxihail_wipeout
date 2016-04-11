using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/status")]
    public class ServerStatusController : BaseApiController
    {
        public ServerStatusService ServerStatusService { get; private set; }
        public ServerStatusController(ServerStatusService serverStatusService)
        {
            ServerStatusService = serverStatusService;
        }

        [HttpGet, Auth(Role = RoleName.Support)]
        public async Task<IHttpActionResult> Status()
        {
            var status = await ServerStatusService.GetServiceStatus();

            return GenerateActionResult(status);
        }

    }
}
