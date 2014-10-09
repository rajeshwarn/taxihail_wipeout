#region

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using CustomerPortal.Web.Services.Impl;
using log4net;

#endregion

namespace CustomerPortal.Web.Controllers.API
{
    [Authorize(Roles = RoleName.Admin)]
    public class PackageController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PackageController));

        [HttpPost]
        [Route("api/packages")]
        public async Task<HttpResponseMessage> Post()
        {
            try
            {
                var stream = await Request.Content.ReadAsStreamAsync();

                var fileName = Request.Content.Headers.ContentDisposition.FileName.TrimEnd('"').TrimStart('"');

                Log.Debug("Filename to be saved : " + fileName);

                var manager = new PackagesManager();
                manager.Delete(fileName);
                manager.Save(fileName, stream);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpGet]
        [Route("api/packages")]
        public string[] GetList()
        {
            var packagesManager = new PackagesManager();
            return packagesManager.GetAll().Select(Path.GetFileName).ToArray();
        }

        [HttpGet]
        [Route("api/packages")]
        public HttpResponseMessage GetPackage(string fileName)
        {
            string localFilePath;
            int fileSize;

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            response.Content =
                new StreamContent(new FileStream(Path.Combine(new PackagesManager().GetFolderPath(), fileName),
                    FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;
            return response;
        }
    }
}