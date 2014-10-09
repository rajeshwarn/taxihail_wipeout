#region

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Cupertino;
using CustomerPortal.Web.Areas.Admin.Models;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers.API
{
    [Authorize(Roles = RoleName.Admin)]
    public class AppleDevCenterController : ApiController
    {
        private readonly Agent _agent;

        public AppleDevCenterController()
        {
            _agent = new Agent();
        }

        [Route("api/admin/appleDevCenter/testLogin")]
        public HttpResponseMessage Post(TestAppleDevCenterLoginRequest request)
        {
            var result = _agent.Login(request.Username, request.Password, request.Team);

            return result.IsSuccessful
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : Request.CreateErrorResponse(HttpStatusCode.BadRequest, result.ErrorMessage);
        }

        [Route("api/admin/appleDevCenter/downloadProfile")]
        public HttpResponseMessage Post(DownloadProvisioningProfileRequest request)
        {
            var result = _agent.DownloadProfile(request.Username, request.Password, request.Team, request.AppId,
                request.AdHoc);

            if (!result.IsSuccessful)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, result.ErrorMessage);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StreamContent(result.FileStream)};
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = result.FileName
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return response;
        }
    }
}