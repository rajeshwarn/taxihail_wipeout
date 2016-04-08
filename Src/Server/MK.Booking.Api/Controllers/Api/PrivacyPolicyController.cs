using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api
{
    public class PrivacyPolicyController : BaseApiController
    {
        public PrivacyPolicyService PrivacyPolicyService { get; private set; }

        public PrivacyPolicyController(ICommandBus commandBus, ICompanyDao companyDao)
        {
            PrivacyPolicyService = new PrivacyPolicyService(companyDao, commandBus);
        }

        [HttpGet, Route("api/v2/privacypolicy")]
        public IHttpActionResult GetPrivacyPolicy()
        {
            var result = PrivacyPolicyService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/privacypolicy"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult UpdatePrivacyPolicy([FromBody]PrivacyPolicyRequest request)
        {
            PrivacyPolicyService.Post(request);

            return Ok();
        }
    }
}
