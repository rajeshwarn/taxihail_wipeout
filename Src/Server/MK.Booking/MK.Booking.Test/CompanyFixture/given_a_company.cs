using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_company
    {
        private EventSourcingTestHelper<Company> sut;
        readonly Guid _companyId = Guid.NewGuid();

        [SetUp]
        public void given_a_company_setup()
        {
            this.sut = new EventSourcingTestHelper<Company>();
            this.sut.Setup(new CompanyCommandHandler(this.sut.Repository));
            this.sut.Given(new CompanyCreated { SourceId = _companyId });
            this.sut.Given(new AppSettingsAdded() { Key = "Key.Default", Value = "Value.Default" });
        }

        [Test]
        public void when_creating_two_default_rates()
        {
            var rateId = Guid.NewGuid();

            this.sut.Given(new TariffCreated
            {
                SourceId = _companyId,
                TariffId = rateId,
                FlatRate = 3.50m,
                DistanceMultiplicator = 1.1,
                TimeAdjustmentFactor = 1.2,
                PricePerPassenger = 1.3m,
                Type = TariffType.Default
            });

            Assert.Throws<InvalidOperationException>(() => this.sut.When(new CreateTariff
            {
                CompanyId = _companyId,
                TariffId = rateId,
                FlatRate = 3.50m,
                DistanceMultiplicator = 1.1,
                TimeAdjustmentFactor = 1.2,
                PricePerPassenger = 1.3m,
                Type = TariffType.Default
            }));
        }

        [Test]
        public void when_creating_a_new_rate()
        {
            var tariffId = Guid.NewGuid();

            this.sut.When(new CreateTariff
                              {
                                  CompanyId = _companyId,
                                  TariffId = tariffId,
                                  Name = "Week-End", 
                                  FlatRate = 3.50m,
                                  DistanceMultiplicator = 1.1,
                                  TimeAdjustmentFactor = 1.2,
                                  PricePerPassenger = 1.3m,
                                  DaysOfTheWeek = DayOfTheWeek.Saturday | DayOfTheWeek.Sunday,
                                  StartTime = DateTime.Today.AddHours(12).AddMinutes(30),
                                  EndTime = DateTime.Today.AddHours(20),
                                  Type = TariffType.Recurring
                              });

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (TariffCreated) sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(tariffId, evt.TariffId);
            Assert.AreEqual("Week-End", evt.Name);
            Assert.AreEqual(3.50, evt.FlatRate);
            Assert.AreEqual(1.1, evt.DistanceMultiplicator);
            Assert.AreEqual(1.2, evt.TimeAdjustmentFactor);
            Assert.AreEqual(1.3m, evt.PricePerPassenger);
            Assert.AreEqual(DayOfTheWeek.Saturday, evt.DaysOfTheWeek & DayOfTheWeek.Saturday);
            Assert.AreEqual(DayOfTheWeek.Sunday, evt.DaysOfTheWeek & DayOfTheWeek.Sunday);
            Assert.AreEqual(12, evt.StartTime.Hour);
            Assert.AreEqual(30, evt.StartTime.Minute);
            Assert.AreEqual(20, evt.EndTime.Hour);
            Assert.AreEqual(00, evt.EndTime.Minute);
            Assert.AreEqual(TariffType.Recurring, evt.Type);

        }

       [Test]
        public void when_appsettings_added_successfully()
        {
            this.sut.When(new AddAppSettings() { CompanyId = _companyId,  Key = "Key.hi", Value = "Value.hi" });

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (AppSettingsAdded)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual("Key.hi", evt.Key);
            Assert.AreEqual("Value.hi", evt.Value);
        }

        [Test]
        public void when_appsettings_updated_successfully()
        {
            this.sut.When(new UpdateAppSettings() { CompanyId = _companyId, Key = "Key.Default", Value = "Value.newValue" });

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (AppSettingsUpdated)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual("Key.Default", evt.Key);
            Assert.AreEqual("Value.newValue", evt.Value);

        }

    }
}
