using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Admin;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class ExportDataController : BaseApiController
    {
        public ExportDataService ExportDataService { get; private set; }

        public ExportDataController(ExportDataService exportDataService)
        {
            ExportDataService = exportDataService;
        }

        [HttpPost, Route("api/admin/export/{target}"), Auth(Role = RoleName.Support)]
        public IHttpActionResult ExportData(DataType target, [FromBody] ExportDataRequest request)
        {
            request.Target = target;

            var result = ExportDataService.Post(request);

            if (result.None())
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result.ToCsv())
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/octet-stream"),
                        ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = "export.csv",
                            Name = "export.csv"
                        }
                    }
                }
            };

            return ResponseMessage(httpResponseMessage);
        }
    }
}
