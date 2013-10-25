using System.Linq;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;
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
            var sut = new ReferenceDataServiceClient(BaseUrl, SessionId, "Test");
            var data = sut.GetReferenceData();

            Assert.IsNotEmpty(data.Result.CompaniesList);
            Assert.IsNotEmpty(data.Result.VehiclesList);
            Assert.IsNotEmpty(data.Result.PaymentsList);

            data.Result.VehiclesList.All(v => data.Result.CompaniesList.Any(c => v.Parent == c));
            data.Result.PaymentsList.All(v => data.Result.CompaniesList.Any(c => v.Parent == c));
            

        }
    }
}