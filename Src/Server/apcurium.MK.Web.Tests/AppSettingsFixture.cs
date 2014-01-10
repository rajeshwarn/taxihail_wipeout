using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using NUnit.Framework;
using ServiceStack.Text;

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
        public async override void Setup()
        {
            base.Setup();
            await CreateAndAuthenticateTestAdminAccount();
            new AdministrationServiceClient(BaseUrl, SessionId, "Test").AddOrUpdateAppSettings(new ConfigurationsRequest
            {
                AppSettings = new Dictionary<string, string> {{"Key.DefaultSetupWeb", "Value.DefaultSetupWeb"}}
            });
        }

        [Test]
        public void UpdateAppSettings()
        {
            new AdministrationServiceClient(BaseUrl, SessionId, "Test").AddOrUpdateAppSettings(new ConfigurationsRequest
            {
                AppSettings = new Dictionary<string, string> {{"Key.DefaultSetupWeb", "Value.DefaultSetupWebUpdated"}}
            });
        }
    }

    [Ignore]
    public class AppSettingsSerializationFixture
    {
        [Test]
        public void TestDictionarySerialization()
        {
            var dict = new Dictionary<string, string> {{"key", "value"}};

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