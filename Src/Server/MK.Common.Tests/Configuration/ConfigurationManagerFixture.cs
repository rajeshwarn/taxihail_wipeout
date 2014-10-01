using System.Collections.Generic;
using System.Data.Entity;
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
        public void When_receiving_a_setting_then_object_is_populated_with_value()
        {
            _appSettingsFromDatabase.Clear();
            _appSettingsFromDatabase.Add(new AppSetting("ApplicationName", "Test"));

            _sut.Load();

            Assert.That(_sut.Data.ApplicationName, Is.EqualTo("Test"));
        }
    }
}