#region

using CustomerPortal.Web.Areas.Customer.Controllers;
using CustomerPortal.Web.Domain;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Services;
using Moq;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Admin.Controllers.GraphicsControllerFixture
{
    [TestFixture]
    public class GraphicsControllerFixture
    {
        [SetUp]
        public void Setup()
        {
            Company = new Company
            {
                Id = "TaxiDiamond",
                CompanyName = "Taxi Diamond",
            };
            ServiceMock = new Mock<ICompanyService>();
            ServiceMock.Setup(x => x.GetCompany()).Returns(Company);
            GraphicsManagerMock = new Mock<IFileManager>();
            Graphics = GraphicsManagerMock.Object;
            Sut = new GraphicsController
            {
                Service = ServiceMock.Object,
                Graphics = Graphics
            };
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            AutoMapperConfig.RegisterMaps();
        }

        protected IFileManager Graphics { get; set; }

        protected Mock<IFileManager> GraphicsManagerMock { get; set; }
        protected Company Company;
        protected Mock<ICompanyService> ServiceMock { get; set; }
        protected GraphicsController Sut { get; set; }

        [Test]
        public void Delete_Post_deletes_graphic_file()
        {
            var result = Sut.Delete("graphic.jpg");

            GraphicsManagerMock.Verify(x => x.Delete("graphic.jpg"));
        }
    }
}