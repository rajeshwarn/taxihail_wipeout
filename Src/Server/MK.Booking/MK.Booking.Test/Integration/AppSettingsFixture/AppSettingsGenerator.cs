using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Test.Integration.AppSettingsFixture
{
    public class given_a_view_model_generator : given_a_config_read_model_database
    {
        protected AppSettingsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new AppSettingsGenerator(() => new ConfigurationDbContext(dbName));
        }
    }

    [TestFixture]
    public class given_no_appsettings : given_a_view_model_generator
    {
        [Test]
        public void when_settings_is_added_to_appsettings_then_list_updated()
        {
            var companyId = Guid.NewGuid();


            this.sut.Handle(new AppSettingsAddedOrUpdated
                                {
                                    SourceId = companyId,
                                    AppSettings = new Dictionary<string, string> { { "Key.Default", "Value.Default" } }
                                });

            using (var context = new ConfigurationDbContext(dbName))
            {
                var list = context.Query<AppSetting>().Where(x => x.Key == "Key.Default");

                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual("Value.Default", dto.Value);

            }
        }
    }

    [TestFixture]
    public class given_an_appsettings : given_a_view_model_generator
    {
        private readonly Guid _companyId = AppConstants.CompanyId;

        [SetUp]
        public void Setup()
        {

            sut.Handle(new AppSettingsAddedOrUpdated()
                           {
                               SourceId = _companyId,
                               AppSettings = new Dictionary<string, string> { { "Key.Default", "Value.Default" } }
                           });

        }

        [Test]
        public void when_appsettings_is_updated_then_list_updated()
        {
            this.sut.Handle(new AppSettingsAddedOrUpdated
                                {
                                    SourceId = _companyId,
                                    AppSettings = new Dictionary<string, string> { { "Key.Default", "Value.Updated" } }
                                });

            using (var context = new ConfigurationDbContext(dbName))
            {
                var list = context.Query<AppSetting>().Where(x => x.Key == "Key.Default");

                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual("Value.Updated", dto.Value);
            }
        }

        [Test]
        public void when_appsettings_is_updated_when_no_settings_before_then_list_updated()
        {
            this.sut.Handle(new AppSettingsAddedOrUpdated
            {
                SourceId = _companyId,
                AppSettings = new Dictionary<string, string> { { "Key.Defaulte", "Value.Updated" } }
            });

            using (var context = new ConfigurationDbContext(dbName))
            {
                var list = context.Query<AppSetting>().Where(x => x.Key == "Key.Defaulte");

                Assert.AreEqual(1, list.Count());
            }
        }
    }
}