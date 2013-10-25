
using System;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class PushNotificationRegistrationFixture: BaseTest
    {
        readonly string _knownDeviceToken = Guid.NewGuid().ToString();
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
            CreateAndAuthenticateTestAdminAccount();
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, "Test");
            sut.Register(_knownDeviceToken, PushNotificationServicePlatform.Android);
        }

        [Test]
        public void RegisterDevice()
        {
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, "Test");
            var deviceToken = Guid.NewGuid().ToString();

            sut.Register(deviceToken, PushNotificationServicePlatform.Android);

            Assert.Inconclusive("Need API to check that device was successfully registered");
        }

        [Test]
        public void UnregisterDevice()
        {
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, "Test");

            sut.Unregister(_knownDeviceToken);

            Assert.Inconclusive("Need API to check that device was successfully unregistered");
        }
    }
}
