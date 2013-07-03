using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.Cmt;

namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    public class CmtAuthFixture : CmtBaseTest
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


        [Test]
        public void when_user_sign_in()
        {
            var sut = new CmtAuthServiceClient(BaseUrl, null);
            var response = sut.Authenticate(TestAccount.Email, TestAccountPassword);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.SessionId);
            Assert.AreEqual(TestAccount.Email, response.UserName);
        }
    }
}
