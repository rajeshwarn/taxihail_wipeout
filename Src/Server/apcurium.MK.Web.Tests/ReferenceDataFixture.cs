#region

using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class ReferenceDataFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

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

        [Test]
        public void Get()
        {
            var sut = new ReferenceDataServiceClient(BaseUrl, SessionId, "Test");
            var data = sut.GetReferenceData();

            Assert.IsNotEmpty(data.Result.CompaniesList);
            Assert.IsNotEmpty(data.Result.VehiclesList);
            Assert.IsNotEmpty(data.Result.PaymentsList);

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
            data.Result.VehiclesList.All(v => data.Result.CompaniesList.Any(c => v.Parent == c));
            data.Result.PaymentsList.All(v => data.Result.CompaniesList.Any(c => v.Parent == c));
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }
    }
}