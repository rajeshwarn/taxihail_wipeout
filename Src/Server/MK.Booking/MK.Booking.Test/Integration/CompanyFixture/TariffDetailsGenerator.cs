#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration.CompanyFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected TariffDetailsGenerator Sut;

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new TariffDetailsGenerator(() => new BookingDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_tariff : given_a_view_model_generator
    {
        [Test]
        public void when_tariff_created_tariff_dto_populated()
        {
            var tariffId = Guid.NewGuid();
            var companyId = Guid.NewGuid();
            Sut.Handle(new TariffCreated
            {
                SourceId = companyId,
                TariffId = tariffId,
                FlatRate = 3.50m,
                KilometricRate = 1.1,
                MarginOfError = 1.2,
                KilometerIncluded = 1.6,
                PassengerRate = 2.0m,
                DaysOfTheWeek = DayOfTheWeek.Saturday | DayOfTheWeek.Sunday,
                StartTime = DateTime.Today.AddHours(12).AddMinutes(55),
                EndTime = DateTime.Today.AddHours(20).AddMinutes(15),
                Type = TariffType.Day,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<TariffDetail>().Where(x => x.Id == tariffId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(tariffId, dto.Id);
                Assert.AreEqual(companyId, dto.CompanyId);
                Assert.AreEqual(3.50m, dto.FlatRate);
                Assert.AreEqual(1.1, dto.KilometricRate);
                Assert.AreEqual(1.2, dto.MarginOfError);
                Assert.AreEqual(1.6, dto.KilometerIncluded);
                Assert.AreEqual(2.0m, dto.PassengerRate);
                Assert.AreEqual((int) (DayOfTheWeek.Saturday | DayOfTheWeek.Sunday), dto.DaysOfTheWeek);
                Assert.AreEqual(12, dto.StartTime.Hour);
                Assert.AreEqual(55, dto.StartTime.Minute);
                Assert.AreEqual(20, dto.EndTime.Hour);
                Assert.AreEqual(15, dto.EndTime.Minute);
                Assert.AreEqual((int) TariffType.Day, dto.Type);
            }
        }
    }

    [TestFixture]
    public class given_a_tariff : given_a_view_model_generator
    {
        [SetUp]
        public void SetUp()
        {
            Sut.Handle(new TariffCreated
            {
                SourceId = (_companyId = Guid.NewGuid()),
                TariffId = (_tariffId = Guid.NewGuid()),
                DaysOfTheWeek = DayOfTheWeek.Sunday,
                StartTime = DateTime.Today,
                EndTime = DateTime.Today,
                Name = "Rate " + Guid.NewGuid()
            });
        }

        private Guid _companyId;
        private Guid _tariffId;

        [Test]
        public void when_tariff_deleted()
        {
            Sut.Handle(new TariffDeleted
            {
                SourceId = _companyId,
                TariffId = _tariffId
            });

            using (var context = new BookingDbContext(DbName))
            {
                Assert.IsFalse(context.Query<TariffDetail>().Any(x => x.Id == _tariffId));
            }
        }

        [Test]
        public void when_tariff_updated_then_dto_updated()
        {
            Sut.Handle(new TariffUpdated
            {
                SourceId = _companyId,
                TariffId = _tariffId,
                DaysOfTheWeek = DayOfTheWeek.Tuesday,
                KilometricRate = 19,
                FlatRate = 20,
                PassengerRate = 21,
                MarginOfError = 22,
                KilometerIncluded = 26,
                StartTime = DateTime.Today.AddHours(23),
                EndTime = DateTime.Today.AddHours(24),
                Name = "Updated Rate",
            });

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<TariffDetail>().Where(x => x.Id == _tariffId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(_tariffId, dto.Id);
                Assert.AreEqual(_companyId, dto.CompanyId);
                Assert.AreEqual(20, dto.FlatRate);
                Assert.AreEqual(19, dto.KilometricRate);
                Assert.AreEqual(22, dto.MarginOfError);
                Assert.AreEqual(26, dto.KilometerIncluded);
                Assert.AreEqual(21, dto.PassengerRate);
                Assert.AreEqual((int) (DayOfTheWeek.Tuesday), dto.DaysOfTheWeek);
                Assert.AreEqual(23, dto.StartTime.Hour);
                Assert.AreEqual(0, dto.EndTime.Hour);
            }
        }
    }
}