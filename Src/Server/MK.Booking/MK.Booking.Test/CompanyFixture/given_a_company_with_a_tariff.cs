#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_company_with_a_tariff
    {
        [SetUp]
        public void SetUp()
        {
            _sut = new EventSourcingTestHelper<Company>();
            _sut.Setup(new CompanyCommandHandler(_sut.Repository));
            _sut.Given(new CompanyCreated {SourceId = _companyId});
            _sut.Given(new TariffCreated {SourceId = _companyId, TariffId = _tariffRateId, Type = TariffType.Default});
        }

        private EventSourcingTestHelper<Company> _sut;
        private readonly Guid _companyId = Guid.NewGuid();
        private readonly Guid _tariffId = Guid.NewGuid();
        private readonly Guid _tariffRateId = Guid.NewGuid();

        [Test]
        public void when_deleting_default_tariff()
        {
            Assert.Throws<InvalidOperationException>(
                () => _sut.When(new DeleteTariff {CompanyId = _companyId, TariffId = _tariffRateId}));
        }

        [Test]
        public void when_deleting_tariff()
        {
            _sut.When(new DeleteTariff {CompanyId = _companyId, TariffId = _tariffId});

            var evt = _sut.ThenHasSingle<TariffDeleted>();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_tariffId, evt.TariffId);
        }

        [Test]
        public void when_updating_tariff()
        {
            _sut.When(new UpdateTariff
            {
                CompanyId = _companyId,
                TariffId = _tariffId,
                FlatRate = 12m,
                PassengerRate = 13,
                KilometricRate = 14,
                MarginOfError = 15,
                KilometerIncluded = 16,
                StartTime = DateTime.Today.AddHours(16),
                EndTime = DateTime.Today.AddHours(17),
                Name = "Updated",
                DaysOfTheWeek = DayOfTheWeek.Wednesday
            });

            var evt = _sut.ThenHasSingle<TariffUpdated>();
            Assert.AreEqual(_companyId, evt.SourceId);
            Assert.AreEqual(_tariffId, evt.TariffId);
            Assert.AreEqual(12, evt.FlatRate);
            Assert.AreEqual(13, evt.PassengerRate);
            Assert.AreEqual(14, evt.KilometricRate);
            Assert.AreEqual(15, evt.MarginOfError);
            Assert.AreEqual(16, evt.KilometerIncluded);
            Assert.AreEqual("Updated", evt.Name);
            Assert.AreEqual(DayOfTheWeek.Wednesday, evt.DaysOfTheWeek);
            Assert.AreEqual(DateTime.Today.AddHours(16), evt.StartTime);
            Assert.AreEqual(DateTime.Today.AddHours(17), evt.EndTime);
        }
    }
}