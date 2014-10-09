#region

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using CustomerPortal.Web.Areas.Admin.Controllers.API;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Services;
using CustomerPortal.Web.Test.Helpers;
using CustomerPortal.Web.Test.Helpers.Repository;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Admin.Controllers.API
{
    [TestFixture]
    public class VersionControllerFixture
    {
        [SetUp]
        public void Setup()
        {
            Repository = new InMemoryRepository<Company>();
            Clock = new FakeClock(DateTime.UtcNow);
            Packages = new FakeFileManager("C:\\uploads");
            Sut = new VersionController(Clock, Repository, (id, path) => Packages)
            {
                Request = new HttpRequestMessage()
            };

            Repository.Add(new Company
            {
                Id = CompanyId,
                CompanyName = "Taxi Diamond"
            });
        }

        protected IFileManager Packages { get; set; }
        protected Mock<IFileManager> PackageManagerMock { get; set; }
        protected VersionController Sut { get; set; }
        protected InMemoryRepository<Company> Repository { get; set; }
        protected FakeClock Clock { get; set; }
        private const string CompanyId = "TaxiDiamond";

        private StreamContent CreateFileContent(Stream stream, string fileName, string contentType)
        {
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"files\"",
                FileName = "\"" + fileName + "\""
            }; // the extra quotes are key here
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }

        [Test]
        public void API_call_to_Post_creates_version_in_company()
        {
            var data = new CreateNewVersionRequest
            {
                CompanyKey = "Test",
                VersionNumber = "2.0",
                WebsiteUrl = "http://website.url"
            };

            using (var client = new HttpClient())
            {
                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    multipartFormDataContent.Add(new StringContent(JsonConvert.SerializeObject(data)), "data");

                    var ipaContent = new StreamContent(File.OpenRead("c:\\test.ipa"));
                    ipaContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    ipaContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "test.ipa"
                    };
                    multipartFormDataContent.Add(ipaContent);

                    var apkContent = new StreamContent(File.OpenRead("c:\\test.apk"));
                    apkContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    apkContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "test.apk"
                    };
                    multipartFormDataContent.Add(apkContent);

                    var result =
                        client.PostAsync("http://localhost:2287/api/admin/version", multipartFormDataContent).Result;
                    Console.WriteLine(result);
                }
            }
        }
    }
}