using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Web.Tests
{
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
        }
    }
}