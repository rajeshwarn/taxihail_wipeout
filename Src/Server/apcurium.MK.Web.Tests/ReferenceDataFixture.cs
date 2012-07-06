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
        public new void Setup()
        {
            base.Setup();
        }

        [TestFixtureTearDown]
        public new void TearDown()
        {
            base.TearDown();
        } 

        [Test]
        public void Get()
        {
            var sut = new ReferenceDataServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
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