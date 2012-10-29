using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client;
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
            sut.AddAppSettings(new AppSettingsRequest()
                                   {
                                       Key = "Key.DefaultSetupWeb",
                                       Value = "Value.DefaultSetupWeb"
                                   });
        }

        [Test]
        public void AddAppSettings()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId);


            sut.AddAppSettings(new AppSettingsRequest()
                                   {
                                       Key = "Key.DefaultWeb",
                                       Value = "Value.DefaultWeb"
                                   });

            var settings = sut.GetAllAppSettings();

            Assert.AreEqual(1, settings.Count(x => x.Key.Equals("Key.DefaultWeb")));
            var setting = settings.Single(x => x.Key.Equals("Key.DefaultWeb"));
            Assert.AreEqual("Value.DefaultWeb", setting.Value);
        }

        [Test]
        public void UpdateAppSettings()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId);

            sut.UpdateAppSettings(new AppSettingsRequest()
                                      {
                                          Key = "Key.DefaultSetupWeb",
                                          Value = "Value.DefaultSetupWebUpdated"
                                      });

            var settings = sut.GetAllAppSettings();

            Assert.AreEqual(1, settings.Count(x => x.Key.Equals("Key.DefaultSetupWeb")));
            var setting = settings.Single(x => x.Key.Equals("Key.DefaultSetupWeb"));
            Assert.AreEqual("Value.DefaultSetupWebUpdated", setting.Value);

        }

        [Test]
        public void UpdateAppSettingsWithInvalidData()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId);

            Assert.AreEqual(HttpStatusCode.Conflict.ToString(), sut
                                                         .UpdateAppSettings(new AppSettingsRequest()
                                                                                {
                                                                                    Key = "Key.DefaultSetewfupWeb",
                                                                                    Value =
                                                                                        "Value.DefaulSetuptWebUpdated"
                                                                                }).ToString(CultureInfo.InvariantCulture));


        }
    }
}