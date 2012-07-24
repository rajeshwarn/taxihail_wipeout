using System.Linq;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class ReferenceDataFixture : BaseTest
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void Get()
        {
            var sut = new ReferenceDataServiceClient(BaseUrl);
            var data = sut.GetReferenceData();
            Assert.IsNotEmpty(data.CompaniesList);
            Assert.IsNotEmpty(data.VehiclesList);
            Assert.IsNotEmpty(data.PaymentsList);
            Assert.IsNotEmpty(data.PickupCityList);
            Assert.IsNotEmpty(data.DropoffCityList);

            data.VehiclesList.All(v => data.CompaniesList.Any(c => v.Parent == c));
            data.PaymentsList.All(v => data.CompaniesList.Any(c => v.Parent == c));
            data.PickupCityList.All(v => data.CompaniesList.Any(c => v.Parent == c));
            data.DropoffCityList.All(v => data.CompaniesList.Any(c => v.Parent == c));            
            

        }
    }
}