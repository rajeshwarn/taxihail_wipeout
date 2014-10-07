#region

using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Services.Impl;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Controllers.API
{
    [Authorize(Roles = RoleName.Admin)]
    public class CompanyController : ApiController
    {
        private readonly IRepository<Company> _repository;
        private readonly IRepository<DefaultCompanySetting> _repositoryDefaultSettings;

        public CompanyController(IRepository<Company> repository, IRepository<DefaultCompanySetting> repositoryDefaultSettings)
        {
            _repository = repository;
            _repositoryDefaultSettings = repositoryDefaultSettings;
        }

        public CompanyController()
            : this(new MongoRepository<Company>(), new MongoRepository<DefaultCompanySetting>())
        {
        }

        // GET api/company
        public IEnumerable Get()
        {
            var companies = _repository.ToList();


            return companies.Select(ApplyDefaults).ToArray();
        }

        // GET api/company/5
        public Company Get(string id)
        {
            var company = _repository.GetById(id);
            ApplyDefaults(company);
            return company;
        }

        private Company ApplyDefaults(Company company)
        {
            foreach (var defaultSetting in _repositoryDefaultSettings)
            {
                if (company.CompanySettings.All(c => c.Key != defaultSetting.Id))
                {
                    company.CompanySettings.Add(new CompanySetting
                    {
                        Key = defaultSetting.Id,
                        Value = defaultSetting.Value,
                        IsClientSetting = defaultSetting.IsClient
                    });
                }
            }
            return company;
        }

        // GET api/company/5/files
        [Route("api/company/{id}/files")]
        public HttpResponseMessage GetCompanyFiles(string id, string type)
        {
            if (String.IsNullOrEmpty(id))
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            var zipFile = Path.Combine(GetEmptyTempFolder(), "out.zip");
            var fileManager = GetFileManager(type, id);
            ZipFile.CreateFromDirectory(fileManager.GetFolderPath(), zipFile);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new FileStream(zipFile, FileMode.Open, FileAccess.Read))
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(zipFile)
            };

            return response;
        }

        private string GetEmptyTempFolder()
        {
            var tempPath = HostingEnvironment.MapPath("~/App_Data/tempzip/" + Guid.NewGuid() + "/");

            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }

            Directory.CreateDirectory(tempPath);

            return tempPath;
        }

        private FileManagerBase GetFileManager(string type, string id)
        {
            switch (type)
            {
                case "assets":
                    return new AssetsManager(id);
                case "webtheme":
                    return new WebThemeFilesManager(id);
            }
            throw new ArgumentException("file manager type not recognized");
        }
    }
}