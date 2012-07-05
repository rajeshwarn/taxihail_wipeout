using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class OrderStatusFixture : BaseTest
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
            var sut = new OrderStatusClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            //var data = sut.GetStatus(123);
            Assert.Inconclusive("Pas implémenter voir service");
        }
    }
}