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
    public class given_a_tariff
    {
        private EventSourcingTestHelper<Company> sut;
        readonly Guid _companyId = Guid.NewGuid();
        readonly Guid _tariffId = Guid.NewGuid();
        readonly Guid _tariffRateId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            this.sut = new EventSourcingTestHelper<Company>();
            this.sut.Setup(new CompanyCommandHandler(this.sut.Repository));
            this.sut.Given(new CompanyCreated { SourceId = _companyId });
            this.sut.Given(new TariffCreated { SourceId = _companyId, TariffId = _tariffRateId, Type = TariffType.Default });
        }

        [Test]
        public void when_updating_tariff()
        {
            sut.When(new UpdateTariff
                         {
                             CompanyId = _companyId,
                             TariffId = _tariffId,
                             FlatRate = 12m,
                             PassengerRate = 13,
                             KilometricRate = 14,
                             MarginOfError = 15,
                             StartTime = DateTime.Today.AddHours(16),
                             EndTime = DateTime.Today.AddHours(17),
                             Name = "Updated",
                             DaysOfTheWeek = DayOfTheWeek.Wednesday
                         });

            Assert.AreEqual(1, sut.Events.Count);
            Assert.IsInstanceOf<TariffUpdated>(sut.Events.Single());
            var evt = (TariffUpdated)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_tariffId, evt.TariffId);
            Assert.AreEqual(12, evt.FlatRate);
            Assert.AreEqual(13, evt.PassengerRate);
            Assert.AreEqual(14, evt.KilometricRate);
            Assert.AreEqual(15, evt.MarginOfError);
            Assert.AreEqual("Updated", evt.Name);
            Assert.AreEqual(DayOfTheWeek.Wednesday, evt.DaysOfTheWeek);
            Assert.AreEqual(DateTime.Today.AddHours(16), evt.StartTime);
            Assert.AreEqual(DateTime.Today.AddHours(17), evt.EndTime);

        }

        [Test]
        public void when_deleting_tariff()
        {
            sut.When(new DeleteTariff{ CompanyId = _companyId, TariffId = _tariffId});
            Assert.AreEqual(1, sut.Events.Count);
            Assert.IsInstanceOf<TariffDeleted>(sut.Events.Single());
            var evt = (TariffDeleted)sut.Events.Single();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_tariffId, evt.TariffId);
        }

        [Test]
        public void when_deleting_default_tariff()
        {
            Assert.Throws<InvalidOperationException>(
                () => sut.When(new DeleteTariff {CompanyId = _companyId, TariffId = _tariffRateId}));

        }

        
    }
}
