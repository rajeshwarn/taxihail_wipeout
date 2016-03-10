using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/status")]
    public class ServerStatusController : BaseApiController
    {
        public ServerStatusService ServerStatusService { get; }
        public ServerStatusController(IServerSettings serverSettings,
            IIBSServiceProvider ibsProvider,
            ITaxiHailNetworkServiceClient networkService,
            IOrderStatusUpdateDao statusUpdaterDao)
        {
            ServerStatusService = new ServerStatusService(serverSettings, ibsProvider, Logger, networkService, statusUpdaterDao);
        }

        [HttpGet, Auth(Role = RoleName.Support)]
        public async Task<IHttpActionResult> Status()
        {
            var status = await ServerStatusService.GetServiceStatus();

            return GenerateActionResult(status);
        }

    }
}
