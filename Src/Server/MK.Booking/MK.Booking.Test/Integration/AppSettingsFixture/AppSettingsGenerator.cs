#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration.AppSettingsFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_view_model_generator : given_a_config_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected AppSettingsGenerator Sut;

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new AppSettingsGenerator(() => new ConfigurationDbContext(DbName), new DummyConfigManager());
        }
    }

    [TestFixture]
    public class given_no_appsettings : given_a_view_model_generator
    {
        [Test]
        public void when_settings_is_added_to_appsettings_then_list_updated()
        {
            var companyId = Guid.NewGuid();


            Sut.Handle(new AppSettingsAddedOrUpdated
            {
                SourceId = companyId,
                AppSettings = new Dictionary<string, string> { { "AboutUsUrl", "DefaultUrl" } }
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var list = context.Query<AppSetting>().Where(x => x.Key == "AboutUsUrl");

                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual("DefaultUrl", dto.Value);
            }
        }
    }

    [TestFixture]
    public class given_an_appsettings : given_a_view_model_generator
    {
        [SetUp]
        public void Setup()
        {
            Sut.Handle(new AppSettingsAddedOrUpdated
            {
                SourceId = _companyId,
                AppSettings = new Dictionary<string, string> { { "AboutUsUrl", "DefaultUrl" } }
            });
        }

        private readonly Guid _companyId = AppConstants.CompanyId;

        [Test]
        public void when_appsettings_is_updated_then_list_updated()
        {
            Sut.Handle(new AppSettingsAddedOrUpdated
            {
                SourceId = _companyId,
                AppSettings = new Dictionary<string, string> { { "AboutUsUrl", "UpdatedUrl" } }
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var list = context.Query<AppSetting>().Where(x => x.Key == "AboutUsUrl");

                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual("UpdatedUrl", dto.Value);
            }
        }

        [Test]
        public void when_appsettings_is_updated_when_no_settings_before_then_list_updated()
        {
            Sut.Handle(new AppSettingsAddedOrUpdated
            {
                SourceId = _companyId,
                AppSettings = new Dictionary<string, string> { { "AboutUsUrl", "UpdatedUrl" } }
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var list = context.Query<AppSetting>().Where(x => x.Key == "AboutUsUrl");

                Assert.AreEqual(1, list.Count());
            }
        }
    }
}