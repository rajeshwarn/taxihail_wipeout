using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class ReferenceDataFixture : BaseTest
    {
        [SetUp]
        public async override Task Setup()
        {
            await base.Setup();
        }

        [TestFixtureSetUp]
        public async override Task TestFixtureSetup()
        {
            await base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [Test]
        public async void Get()
        {
            var sut = new ReferenceDataServiceClient(BaseUrl, SessionId, "Test");
            var data = await sut.GetReferenceData();

            Assert.IsNotEmpty(data.CompaniesList);
            Assert.IsNotEmpty(data.VehiclesList);
            Assert.IsNotEmpty(data.PaymentsList);

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
            data.VehiclesList.All(v => data.CompaniesList.Any(c => v.Parent == c));
            data.PaymentsList.All(v => data.CompaniesList.Any(c => v.Parent == c));
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }
    }
}