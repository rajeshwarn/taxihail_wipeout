using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    public class PrivacyPolicyController : BaseApiController
    {
        public PrivacyPolicyService PrivacyPolicyService { get; private set; }

        public PrivacyPolicyController(PrivacyPolicyService privacyPolicyService)
        {
            PrivacyPolicyService = privacyPolicyService;
        }

        [HttpGet, Route("api/privacypolicy")]
        public IHttpActionResult GetPrivacyPolicy()
        {
            var result = PrivacyPolicyService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/privacypolicy"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult UpdatePrivacyPolicy([FromBody]PrivacyPolicyRequest request)
        {
            PrivacyPolicyService.Post(request);

            return Ok();
        }
    }
}
