#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Http;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Extensions;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Services;
using CustomerPortal.Web.Services.Impl;
using MongoRepository;
using Newtonsoft.Json;
using Version = CustomerPortal.Web.Entities.Version;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers.API
{
//    [Authorize(Roles = RoleName.Admin)]
    public class VersionController : ApiController
    {
        private readonly IClock _clock;
        private readonly Func<string, string, IFileManager> _packageManagerFactory;
        private readonly IRepository<Company> _repository;

        public VersionController(IClock clock, IRepository<Company> repository,
            Func<string, string, IFileManager> packageManagerFactory)
        {
            _clock = clock;
            _repository = repository;
            _packageManagerFactory = packageManagerFactory;
        }

        public VersionController()
            : this(Clock.Instance, new MongoRepository<Company>(), (id, version) => new PackageManager(id, version))
        {
        }

        [Route("api/admin/version")]
        public async Task<HttpResponseMessage> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This request is not properly formatted");
            }

            var provider = new MultipartFormDataStreamProvider(HostingEnvironment.MapPath("~/App_Data"));

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                var data = JsonConvert.DeserializeObject<CreateNewVersionRequest>(provider.FormData["data"]);

                if (String.IsNullOrEmpty(data.CompanyKey) || String.IsNullOrEmpty(data.VersionNumber) ||
                    String.IsNullOrEmpty(data.WebsiteUrl))
                {
                    DeleteTemporaryFiles(provider.FileData);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing parameters");
                }

                var company = _repository.FirstOrDefault(x => x.CompanyKey == data.CompanyKey);
                if (company == null)
                {
                    DeleteTemporaryFiles(provider.FileData);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Company does not exist");
                }

                var existingVersion = company.Versions.FirstOrDefault(x => x.Number == data.VersionNumber);
                if (existingVersion != null)
                {
                    company.Versions.Remove(existingVersion);
                    _repository.Update(company);

                    var packages = _packageManagerFactory.Invoke(company.Id, existingVersion.Number);
                    packages.DeleteAll();
                }

                var version = new Version
                {
                    VersionId = Guid.NewGuid().ToString(),
                    WebsiteUrl = data.WebsiteUrl,
                    CreatedOn = _clock.UtcNow,
                    Number = data.VersionNumber
                };

                foreach (var file in provider.FileData)
                {
                    if (IsAPK(file.Headers.ContentDisposition.FileName))
                    {
                        version.ApkFilename = file.Headers.ContentDisposition.FileName;
                    }
                    else if ( IsAppStoreIpa(file.Headers.ContentDisposition.FileName))
                    {
                        version.IpaAppStoreFilename = file.Headers.ContentDisposition.FileName;
                    }
                    else if ((!IsAppStoreIpa(file.Headers.ContentDisposition.FileName)) && (IsIpa(file.Headers.ContentDisposition.FileName)))
                    {
                        version.IpaFilename = file.Headers.ContentDisposition.FileName;
                    }

                    var path =
                        ((PackageManager) _packageManagerFactory.Invoke(company.Id, data.VersionNumber)).GetFolderPath();
                    File.Move(file.LocalFileName, Path.Combine(path, file.Headers.ContentDisposition.FileName));
                }

                company.Versions.Add(version);
                _repository.Update(company);

                //send email for the new version
                var appName = company.Application.AppName ?? company.CompanyName;
                var subject = appName + " version " + version.Number;
                var urlVersionDistribution = string.Format(Url.Link("Distribution", new { id = company.Id }) + "?versionId={0}", version.VersionId);
                var body = "Url of the version: " + urlVersionDistribution;
                WebMail.Send("taxihail.build@apcurium.com", subject, body);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        private void DeleteTemporaryFiles(IEnumerable<MultipartFileData> fileData)
        {
            foreach (var file in fileData)
            {
                File.Delete(file.LocalFileName);
            }
        }

        private bool IsAPK(string fileName)
        {
            if (fileName.Contains(".apk"))
            {
                return true;
            }

            return false;
        }

        private bool IsAppStoreIpa(string fileName)
        {
            if (fileName.ToLower().Contains("appstore.ipa"))
            {
                return true;
            }

            return false;
        }

        private bool IsIpa(string fileName)
        {
            if (fileName.Contains(".ipa"))
            {
                return true;
            }

            return false;
        }

    }
}