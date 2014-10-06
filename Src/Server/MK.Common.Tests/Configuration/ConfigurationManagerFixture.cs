using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration.Impl;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Common.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationManagerFixture
    {
        private ConfigurationManager _sut;
        private List<AppSetting> _appSettingsFromDatabase = new List<AppSetting>();

        [SetUp]
        public void Setup()
        {
            var dbContext = new Mock<ConfigurationDbContext>("Test");
            dbContext
                .Setup(x => x.Query<AppSetting>())
                .Returns(() => _appSettingsFromDatabase.AsQueryable());

            _sut = new ConfigurationManager(() => dbContext.Object, null);
        }

        [Test]
        public void When_receiving_a_setting_from_base_class_then_object_is_populated_with_value()
        {
            _appSettingsFromDatabase.Clear();
            _appSettingsFromDatabase.Add(new AppSetting("Client.TutorialEnabled", "true"));

            _sut.Load();

            Assert.That(_sut.Data.TutorialEnabled, Is.EqualTo(true));
        }

        [Test]
        public void When_receiving_a_setting_from_base_class_with_client_suffix_then_object_is_populated_with_value()
        {
            _appSettingsFromDatabase.Clear();
            _appSettingsFromDatabase.Add(new AppSetting("TwitterConsumerSecret", "Test"));

            _sut.Load();

            Assert.That(_sut.Data.TwitterConsumerSecret, Is.EqualTo("Test"));
        }

        [Test]
        public void When_receiving_a_setting_then_object_is_populated_with_value()
        {
            _appSettingsFromDatabase.Clear();
            _appSettingsFromDatabase.Add(new AppSetting("Admin.CompanySettings", "Yes"));

            _sut.Load();

            Assert.That(_sut.ServerData.Admin.CompanySettings, Is.EqualTo("Yes"));
        }

        [Test]
        public void When_receiving_a_setting_in_nested_property_then_object_is_populated_with_value()
        {
            _appSettingsFromDatabase.Clear();
            _appSettingsFromDatabase.Add(new AppSetting("Smtp.Host", "google.com"));

            _sut.Load();

            Assert.That(_sut.ServerData.Smtp.Host, Is.EqualTo("google.com"));
        }

        [Test]
        public void When_receiving_a_setting_in_doubly_nested_property_then_object_is_populated_with_value()
        {
            _appSettingsFromDatabase.Clear();
            _appSettingsFromDatabase.Add(new AppSetting("Smtp.Credentials.Username", "test123456"));

            _sut.Load();

            Assert.That(_sut.ServerData.Smtp.Credentials.Username, Is.EqualTo("test123456"));
        }

        [Test]
        public void When_receiving_a_setting_in_nested_partial_class_property_then_object_is_populated_with_value()
        {
            _appSettingsFromDatabase.Clear();
            _appSettingsFromDatabase.Add(new AppSetting("GCM.SenderId", "123"));
            _appSettingsFromDatabase.Add(new AppSetting("GCM.APIKey", "456"));

            _sut.Load();

            Assert.That(_sut.Data.GCM.SenderId, Is.EqualTo("123"));
            Assert.That(_sut.Data.GCM.APIKey, Is.EqualTo("456"));
            Assert.That(_sut.ServerData.GCM.SenderId, Is.EqualTo("123"));
            Assert.That(_sut.ServerData.GCM.APIKey, Is.EqualTo("456"));
        }

        [Test]
        public void When_receiving_a_setting_from_client_then_the_two_objects_are_populated_then_object_is_populated_with_value()
        {
            _appSettingsFromDatabase.Clear();
            _appSettingsFromDatabase.Add(new AppSetting("TwitterEnabled", "true"));

            _sut.Load();

            Assert.That(_sut.Data.TwitterEnabled, Is.EqualTo(true));
            Assert.That(_sut.ServerData.TwitterEnabled, Is.EqualTo(true));
        }
    }
}