﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Web.Tests
{
    public class AppSettingsFixture : BaseTest
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
            CreateAndAuthenticateTestAdminAccount();
            var sut = new AdministrationServiceClient(BaseUrl, SessionId);
            sut.AddOrUpdateAppSettings(new ConfigurationsRequest()
                    {
                        AppSettings = new Dictionary<string, string> { { "Key.DefaultSetupWeb", "Value.DefaultSetupWeb" } }
                    });
        }

        //[Test]
        //public void AddAppSettings()
        //{
        //    var sut = new AdministrationServiceClient(BaseUrl, SessionId);

        //    Assert.AreEqual(HttpStatusCode.OK.ToString(), sut.AddAppSettings(new ConfigurationsRequest()
        //            {
        //                AppSettings = new Dictionary<string, string> { { "Key.DefaultWeb", "Value.DefaultWeb" } } 
        //            }).ToString(CultureInfo.InvariantCulture));

        //}

        [Test]
        public void UpdateAppSettings()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId);

            sut.AddOrUpdateAppSettings(new ConfigurationsRequest
                   {
                       AppSettings = new Dictionary<string, string> { { "Key.DefaultSetupWeb", "Value.DefaultSetupWebUpdated" } }
                   });
        }

        //[Test] //No more invalid data with AddOrUpdate
        //public void UpdateAppSettingsWithInvalidData()
        //{
        //    var sut = new AdministrationServiceClient(BaseUrl, SessionId);

        //    Assert.AreEqual(HttpStatusCode.Conflict.ToString(), sut.AddOrUpdateAppSettings(new ConfigurationsRequest()
        //            {
        //                AppSettings = new Dictionary<string, string> { { "Key.DefaultSetewfupWeb", "Value.DefaulSetuptWebUpdated" } }
        //            }).ToString(CultureInfo.InvariantCulture));
        //}
    }

    [Ignore]
    public class AppSettingsSerializationFixture
    {
        [Test]
        public void TestDictionarySerialization()
        {
            var dict = new Dictionary<string, string> { { "key", "value" } };

            var serialization = JsonSerializer.SerializeToString(dict);
            var desrializedDict = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(serialization);

            Assert.That(desrializedDict.Count, Is.EqualTo(1));

            string value;
            desrializedDict.TryGetValue(dict.First().Key, out value);

            Assert.That(value, Is.Not.Null);
            Assert.That(value, Is.EqualTo(dict.First().Value));
        }
    }
}