#region

using System.Web;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Controllers;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Services;
using CustomerPortal.Web.Test.Helpers;
using CustomerPortal.Web.Test.Helpers.Repository;
using Moq;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Admin.Controllers.LayoutControllerFixture
{
    public class LayoutControllerFixture
    {
        private const string CompanyId = "TaxiDiamond";
        protected IFileManager Layouts { get; set; }
        protected Mock<IFileManager> LayoutManagerMock { get; set; }
        protected LayoutController Sut { get; set; }
        protected InMemoryRepository<Company> Repository { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            AutoMapperConfig.RegisterMaps();
        }

        [SetUp]
        public void Setup()
        {
            Repository = new InMemoryRepository<Company>();
            LayoutManagerMock = new Mock<IFileManager>();
            Layouts = LayoutManagerMock.Object;
            Sut = new LayoutController(Repository, id => Layouts);

            Repository.Add(new Company
            {
                Id = CompanyId,
                CompanyName = "Taxi Diamond"
            });
            LayoutManagerMock.Setup(x => x.Delete("layout.jpg")).Returns(true);
        }

        [Test]
        public void Index_Get_asks_for_Index_view()
        {
            var result = Sut.Index(CompanyId);

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult) result;
            Assert.IsInstanceOf<LayoutsViewModel>(viewResult.Model);
            Assert.AreEqual("Taxi Diamond", viewResult.ViewBag.CompanyName);
        }

        [Test]
        public void Index_Post_saves_uploaded_layouts_and_redirects_to_Index()
        {
            // Arrange
            var files = new[]
            {
                new PostedFileStub("layout.jpg")
            };

            // Act
            var result = Sut.Index(CompanyId, files);

            // Assert
            LayoutManagerMock.Verify(x => x.Save(It.IsAny<HttpPostedFileBase>()));

            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            var redirect = (RedirectToRouteResult) result;
            Assert.AreEqual("Index", redirect.RouteValues["action"]);
            Assert.AreEqual(CompanyId, redirect.RouteValues["id"]);
        }

        [Test]
        public void Delete_Post_deletes_file()
        {
            var result = Sut.Delete(CompanyId, "layout.jpg");

            LayoutManagerMock.Verify(x => x.Delete("layout.jpg"));
        }

        [Test]
        public void Delete_Post_redirects_to_Index()
        {
            var result = Sut.Delete(CompanyId, "layout.jpg");

            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            var redirect = (RedirectToRouteResult) result;
            Assert.AreEqual("Index", redirect.RouteValues["action"]);
            Assert.AreEqual(CompanyId, redirect.RouteValues["id"]);
        }

        [Test]
        public void Image_Get_returns_HttpNotFound_result_when_file_does_not_exist()
        {
            // Arrange
            string filepath = null;
            LayoutManagerMock.Setup(x => x.Exists("file.jpg", out filepath)).Returns(false);

            // Act
            var result = Sut.Image(CompanyId, "file.jpg");

            // Assert
            Assert.IsInstanceOf<HttpNotFoundResult>(result);
        }

        [Test]
        public void Image_Get_asks_for_image_file()
        {
            // Arrange
            string filepath = "path/to/file.jpg";
            LayoutManagerMock.Setup(x => x.Exists("file.jpg", out filepath)).Returns(true);

            // Act
            var result = Sut.Image(CompanyId, "file.jpg");

            // Assert
            Assert.IsInstanceOf<FilePathResult>(result);
            var filePathResult = (FilePathResult) result;
            Assert.AreEqual("path/to/file.jpg", filePathResult.FileName);
        }
    }
}