using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Admin;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("api/v2/admin/exclusions")]
    public class ExclusionsController : BaseApiController
    {
        public ExclusionsService ExclusionsService { get; private set; }

        public ExclusionsController(ExclusionsService exclusionsService)
        {
            ExclusionsService = exclusionsService;
        }

        [HttpGet, Auth]
        public IHttpActionResult GetExclusions()
        {
            var result = ExclusionsService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth]
        public IHttpActionResult UpdateExclusions([FromBody]ExclusionsRequest request)
        {
            ExclusionsService.Post(request);

            return Ok();
        }
    }
}
