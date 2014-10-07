#region

using System.Linq;
using CustomerPortal.Web.Areas.Admin.Controllers;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Test.Helpers.Membership;
using CustomerPortal.Web.Test.Helpers.Repository;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Admin.Controllers.MembershipControllerFixture
{
    [TestFixture]
    public class MembershipControllerFixture
    {
        [SetUp]
        public void Setup()
        {
            Repository = new InMemoryRepository<Company>();
            MembershipService = new InMemoryMembershipService();
            Sut = new MembershipController(Repository, MembershipService);

            Repository.Add(new Company
            {
                Id = CompanyId,
                CompanyName = "Taxi Diamond"
            });
        }

        private const string CompanyId = "TaxiDiamond";

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            AutoMapperConfig.RegisterMaps();
        }

        protected MembershipController Sut { get; set; }
        protected InMemoryRepository<Company> Repository { get; set; }
        protected InMemoryMembershipService MembershipService { get; set; }

        [Test]
        public void Create_Post_adds_a_customer_user_to_the_company()
        {
            Sut.Create(new CreateUser
            {
                EmailAddress = "customer@example.net",
                Name = "The Customer",
                Password = "password",
                CompanyId = CompanyId
            });

            var user = MembershipService.Users.FirstOrDefault(x => x.EmailAddress == "customer@example.net");
            Assert.IsNotNull(user);
            CollectionAssert.Contains(user.Roles, RoleName.Customer);
        }
    }
}