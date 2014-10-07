#region

using CustomerPortal.Web.Areas.Customer.Controllers;
using CustomerPortal.Web.Domain;
using CustomerPortal.Web.Entities;
using Moq;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Customer.Controllers.HomeControllerFixture
{
    public class HomeControllerFixtureBase
    {
        protected Company Company;
        protected Mock<ICompanyService> ServiceMock { get; set; }
        protected HomeController Sut { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            AutoMapperConfig.RegisterMaps();
        }

        public virtual void Setup()
        {
            Company = new Company
            {
                Id = "TaxiDiamond",
                CompanyName = "Taxi Diamond",
            };
            ServiceMock = new Mock<ICompanyService>();
            ServiceMock.Setup(x => x.GetCompany()).Returns(Company);
            Sut = new HomeController {Service = ServiceMock.Object};
        }
    }
}