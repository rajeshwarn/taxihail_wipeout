﻿using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

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
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, new PackageInfo());
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
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, new PackageInfo());
            var deviceToken = Guid.NewGuid().ToString();

            await sut.Register(deviceToken, PushNotificationServicePlatform.Android);

            Assert.Inconclusive("Need API to check that device was successfully registered");
        }

        [Test]
        public async void UnregisterDevice()
        {
            var sut = new PushNotificationRegistrationServiceClient(BaseUrl, SessionId, new PackageInfo());

            await sut.Unregister(_knownDeviceToken);

            Assert.Inconclusive("Need API to check that device was successfully unregistered");
        }
    }
}