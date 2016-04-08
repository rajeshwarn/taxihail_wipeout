using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Admin;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("api/v2/admin/export")]
    public class ExportDataController : BaseApiController
    {
        public ExportDataService ExportDataService { get; private set; }

        public ExportDataController(IAccountDao accountDao, IReportDao reportDao, IServerSettings serverSettings, IAppStartUpLogDao appStartUpLogDao, IPromotionDao promotionsDao)
        {
            ExportDataService = new ExportDataService(accountDao, reportDao, serverSettings, appStartUpLogDao, promotionsDao);
        }

        [HttpPost, Route("{target}"), Auth(Role = RoleName.Support)]
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
