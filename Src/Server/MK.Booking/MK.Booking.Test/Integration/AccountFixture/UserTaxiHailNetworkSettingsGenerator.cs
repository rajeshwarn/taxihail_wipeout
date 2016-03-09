using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.AccountFixture
{
    public class given_a_usertaxihailnetworksettings_model_generator : given_a_read_model_database
    {
        private readonly List<ICommand> _commands = new List<ICommand>();
        protected readonly UserTaxiHailNetworkSettingsGenerator Sut;

        public given_a_usertaxihailnetworksettings_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => _commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => _commands.AddRange(x.Select(e => e.Body)));

            Sut = new UserTaxiHailNetworkSettingsGenerator(() => new ConfigurationDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_usertaxihailnetwork_settings_for_account : given_a_usertaxihailnetworksettings_model_generator
    {
        [Test]
        public void when_settings_dont_exist_and_settings_updated_then_dto_populated()
        {
            var accountId = Guid.NewGuid();
            var disabledFleets = new [] {"Apcurium", "TaxiHailDemo"};

            Sut.Handle(new UserTaxiHailNetworkSettingsAddedOrUpdated
            {
                SourceId = accountId,
                IsEnabled = true,
                DisabledFleets = disabledFleets
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var dto = context.UserTaxiHailNetworkSettings.Find(accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(accountId, dto.Id);
                Assert.AreEqual(true, dto.IsEnabled);
                Assert.AreEqual(disabledFleets.Flatten(","), dto.SerializedDisabledFleets);
            }
        }
    }

    [TestFixture]
    public class given_a_usertaxihailnetwork_settings_for_account : given_a_usertaxihailnetworksettings_model_generator
    {
        private readonly Guid _accountId = Guid.NewGuid();

        public given_a_usertaxihailnetwork_settings_for_account()
        {
            Sut.Handle(new UserTaxiHailNetworkSettingsAddedOrUpdated
            {
                SourceId = _accountId,
                IsEnabled = true,
                DisabledFleets = new[] { "Apcurium", "TaxiHailDemo" }
            });
        }

        [Test]
        public void when_settings_exist_and_settings_updated_then_dto_updated()
        {
            var disabledFleets = new[] {"Apcurium"};

            Sut.Handle(new UserTaxiHailNetworkSettingsAddedOrUpdated
            {
                SourceId = _accountId,
                IsEnabled = false,
                DisabledFleets = disabledFleets
            });

            using (var context = new ConfigurationDbContext(DbName))
            {
                var dto = context.UserTaxiHailNetworkSettings.Find(_accountId);

                Assert.NotNull(dto);
                Assert.AreEqual(_accountId, dto.Id);
                Assert.AreEqual(false, dto.IsEnabled);
                Assert.AreEqual(disabledFleets.Flatten(","), dto.SerializedDisabledFleets);
            }
        }
    }
}
