#region

using System;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Customer.Controllers;
using CustomerPortal.Web.Domain;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Test.Helpers;
using Moq;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Customer.Controllers.LayoutControllerFixture
{
    [TestFixture]
    public class LayoutControllerFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            Company = new Company
            {
                Id = "TaxiDiamond",
                CompanyName = "Taxi Diamond",
            };
            Clock = new FakeClock(DateTime.UtcNow);
            ServiceMock = new Mock<ICompanyService>();
            ServiceMock.Setup(x => x.GetCompany()).Returns(Company);
            Sut = new LayoutController {Service = ServiceMock.Object};
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            AutoMapperConfig.RegisterMaps();
        }

        protected FakeClock Clock { get; set; }
        protected Company Company;
        protected Mock<ICompanyService> ServiceMock { get; set; }
        protected LayoutController Sut { get; set; }

        [Test]
        public void Index_Get_asks_for_Index_view()
        {
            var result = Sut.Index();

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult) result;

            Assert.IsInstanceOf<LayoutsViewModel>(viewResult.Model);
        }
    }
}