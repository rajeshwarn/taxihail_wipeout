using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;


namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    [Ignore("CMT - Should this be removed???")]
    public class CmtPreCogFixture : CmtBaseTest
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
        public void when_sending_guide()
        {
            Credentials.AccessToken = "toto";
            Credentials.AccessTokenSecret = "titi";
            Credentials.SessionId = "SessoionId";
            Credentials.AccountId = "one";

            var sut = new CmtPreCogServiceClient(BaseUrl, Credentials);
            var newAccount = new PreCogRequest
                                 {
                                     DestDesc = "moma",
                                     Init = false,
                                     LinkedVehiculeId = "514896",
                                     Type = PreCogType.Guide
                                 };
            sut.Send(newAccount);
        }

    }
}
