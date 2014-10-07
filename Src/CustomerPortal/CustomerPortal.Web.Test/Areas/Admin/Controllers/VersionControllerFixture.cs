#region

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CustomerPortal.Web.Areas.Admin.Controllers;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Services;
using CustomerPortal.Web.Test.Helpers;
using CustomerPortal.Web.Test.Helpers.Repository;
using Moq;
using NUnit.Framework;
using Version = CustomerPortal.Web.Entities.Version;

#endregion

namespace CustomerPortal.Web.Test.Areas.Admin.Controllers
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
            Sut = new VersionController(Repository, Clock, (id, path) => Packages)
            {
                Url =
                    new UrlHelper(
                        new RequestContext(new Mock<HttpContextBase>(MockBehavior.Strict).Object, new RouteData()),
                        new RouteCollection())
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

        [Test]
        public void Create_Get_asks_for_Create_view()
        {
            var result = Sut.Create(CompanyId);

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult) result;
            Assert.IsInstanceOf<Version>(viewResult.Model);
        }

        [Test]
        public void Create_Post_creates_version_in_company()
        {
            // Arrange
            var ipa = new PostedFileStub("file.ipa");
            var apk = new PostedFileStub("file.apk");
            var model = new Version
            {
                Number = "12.34.56.78.90",
                WebsiteUrl = "http://website.url"
            };

            // Act
            Sut.Create(CompanyId, model);

            // Assert
            var company = Repository.GetById(CompanyId);
            Assert.AreEqual(1, company.Versions.Count);

            Sut.CreateIpa(CompanyId, model.Number, ipa);
            Sut.CreateApk(CompanyId, model.Number, apk);

            var version = company.Versions[0];
            Assert.AreEqual("12.34.56.78.90", version.Number);
            Assert.AreEqual("file.ipa", version.IpaFilename);
            Assert.AreEqual("file.apk", version.ApkFilename);
            Assert.AreEqual("http://website.url", version.WebsiteUrl);
            Assert.AreEqual(Clock.UtcNow, version.CreatedOn);

            // Test that files were saved
            string filename;
            Assert.IsTrue(Packages.Exists("file.ipa", out filename));
            Assert.AreEqual("C:\\uploads\\file.ipa", filename);
            Assert.IsTrue(Packages.Exists("file.apk", out filename));
            Assert.AreEqual("C:\\uploads\\file.apk", filename);
        }

        [Test]
        public void Index_Get_asks_for_Index_view()
        {
            var result = Sut.Index(CompanyId);

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult) result;
            Assert.IsInstanceOf<IEnumerable<Version>>(viewResult.Model);
        }
    }
}