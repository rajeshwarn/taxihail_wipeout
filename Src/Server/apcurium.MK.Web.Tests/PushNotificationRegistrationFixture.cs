using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using System.Linq;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class PushNotificationRegistrationFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount().Wait();
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
            sut.Register(_knownDeviceToken, PushNotificationServicePlatform.Android).Wait();
        }

        private readonly string _knownDeviceToken = Guid.NewGuid().ToString();

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
        public async void RegisterDevice()
        {
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
            var deviceToken = Guid.NewGuid().ToString();

            await sut.Register(deviceToken, PushNotificationServicePlatform.Android);

            using (var bookingDbContext = new BookingDbContext("MKWebDev"))
            {
                var registration = bookingDbContext.Set<DeviceDetail>()
                                                    .FirstOrDefault(x => x.DeviceToken == deviceToken);

                Assert.NotNull(registration);
            }
        }

        [Test]
        public async void UnregisterDevice()
        {
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);

            await sut.Unregister(_knownDeviceToken);

            using (var bookingDbContext = new BookingDbContext("MKWebDev"))
            {
                var registration = bookingDbContext.Set<DeviceDetail>()
                                                    .FirstOrDefault(x => x.DeviceToken == _knownDeviceToken);

                Assert.Null(registration);
            }
        }
    }
}