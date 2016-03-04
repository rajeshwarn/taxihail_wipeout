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
        public PrivacyPolicyService PrivacyPolicyService { get; }

        public PrivacyPolicyController(ICommandBus commandBus, ICompanyDao companyDao)
        {
            PrivacyPolicyService = new PrivacyPolicyService(companyDao, commandBus);
        }

        [HttpGet, Route("privacypolicy")]
        public IHttpActionResult GetPrivacyPolicy()
        {
            var result = PrivacyPolicyService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Route("privacypolicy"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult UpdatePrivacyPolicy([FromBody]PrivacyPolicyRequest request)
        {
            PrivacyPolicyService.Post(request);

            return Ok();
        }
    }
}
