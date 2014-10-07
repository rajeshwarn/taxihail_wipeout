#region

using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Controllers;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Test.Helpers.Membership;
using CustomerPortal.Web.Test.Helpers.Repository;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Admin.Controllers.IBSControllerFixture
{
    [TestFixture]
    public class IBSControllerFixture
    {
        [SetUp]
        public void Setup()
        {
            Repository = new InMemoryRepository<Company>();
            MembershipService = new InMemoryMembershipService();
            Sut = new IBSController(Repository);

            Repository.Add(new Company
            {
                Id = CompanyId,
                CompanyName = "Taxi Diamond"
            });
        }

        private const string CompanyId = "TaxiDiamond";
        protected IBSController Sut { get; set; }
        protected InMemoryRepository<Company> Repository { get; set; }
        protected InMemoryMembershipService MembershipService { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            AutoMapperConfig.RegisterMaps();
        }

        [Test]
        public void Index_Get_asks_for_Index_view()
        {
            var result = Sut.Index(CompanyId);

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult) result;
            Assert.IsInstanceOf<IBSSettings>(viewResult.Model);
            Assert.AreEqual("Taxi Diamond", viewResult.ViewBag.CompanyName);
        }
    }
}