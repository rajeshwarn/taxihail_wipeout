using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Admin;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class ExclusionsController : BaseApiController
    {
        public ExclusionsService ExclusionsService { get; private set; }

        public ExclusionsController(ExclusionsService exclusionsService)
        {
            ExclusionsService = exclusionsService;
        }

        [HttpGet, Route("api/admin/exclusions"), Auth]
        public IHttpActionResult GetExclusions()
        {
            var result = ExclusionsService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/admin/exclusions"), Auth]
        public IHttpActionResult UpdateExclusions([FromBody]ExclusionsRequest request)
        {
            ExclusionsService.Post(request);

            return Ok();
        }
    }
}
