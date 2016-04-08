using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api
{
    public class TermsAndConditionsController : BaseApiController
    {
        public TermsAndConditionsService TermsAndConditionsService { get; private set; }
        public TermsAndConditionsController(ICompanyDao companyDao, ICommandBus commandBus)
        {
            TermsAndConditionsService = new TermsAndConditionsService(companyDao, commandBus);
        }

        [HttpGet, Route("api/v2/termsandconditions")]
        public IHttpActionResult Get()
        {
            var isNotModified = TermsAndConditionsService.IsNotModified();

            if (isNotModified)
            {
                return StatusCode(HttpStatusCode.NotModified);
            }

            var currentVersion = TermsAndConditionsService.GetCompanyVersion();
            
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            if (currentVersion.HasValueTrimmed())
            {
                httpResponse.Headers.ETag = new EntityTagHeaderValue("\""+currentVersion+"\"");
            }

            var result = TermsAndConditionsService.Get().ToJson();
            var content = new StringContent(result);

            content.Headers.ContentType.MediaType = "application/json";

            httpResponse.Content = content;

            return ResponseMessage(httpResponse);
        }

        [HttpPost, Route("api/v2/termsandconditions"),Auth(Role = RoleName.Admin)]
        public IHttpActionResult UpdateTerms([FromBody]TermsAndConditionsRequest request)
        {
            TermsAndConditionsService.Post(request);

            return Ok();
        }

        [HttpPost, Route("api/v2/termsandconditions/retrigger"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult ForceTermsAndConditionTriggering()
        {
            TermsAndConditionsService.RetriggerTermsAndConditions();

            return Ok();
        }
    }
}
