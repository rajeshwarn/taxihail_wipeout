using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Test.Integration.CompanyFixture
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected TariffDetailsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new TariffDetailsGenerator(() => new BookingDbContext(dbName));
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
            this.sut.Handle(new TariffCreated
            {
                SourceId = companyId,
                TariffId = tariffId,
                FlatRate = 3.50m,
                KilometricRate = 1.1,
                MarginOfError = 1.2,
                PassengerRate = 2.0m,
                DaysOfTheWeek = DayOfTheWeek.Saturday | DayOfTheWeek.Sunday,
                StartTime = DateTime.Today.AddHours(12).AddMinutes(55),
                EndTime = DateTime.Today.AddHours(20).AddMinutes(15),
                Type = TariffType.Day,
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<TariffDetail>().Where(x => x.Id == tariffId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(tariffId, dto.Id);
                Assert.AreEqual(companyId, dto.CompanyId);
                Assert.AreEqual(3.50m, dto.FlatRate);
                Assert.AreEqual(1.1, dto.KilometricRate);
                Assert.AreEqual(1.2, dto.MarginOfError);
                Assert.AreEqual(2.0m, dto.PassengerRate);
                Assert.AreEqual((int)(DayOfTheWeek.Saturday | DayOfTheWeek.Sunday), dto.DaysOfTheWeek);
                Assert.AreEqual(12, dto.StartTime.Hour);
                Assert.AreEqual(55, dto.StartTime.Minute);
                Assert.AreEqual(20, dto.EndTime.Hour);
                Assert.AreEqual(15, dto.EndTime.Minute);
                Assert.AreEqual((int)TariffType.Day, dto.Type);
            }
        }
    }

    [TestFixture]
    public class given_a_tariff : given_a_view_model_generator
    {
        private Guid _companyId;
        private Guid _tariffId;

        [SetUp]
        public void SetUp()
        {

            this.sut.Handle(new TariffCreated
                                {
                                    SourceId = (_companyId = Guid.NewGuid()),
                                    TariffId = (_tariffId = Guid.NewGuid()),
                                    DaysOfTheWeek = DayOfTheWeek.Sunday,
                                    StartTime = DateTime.Today,
                                    EndTime = DateTime.Today,
                                    Name = "Rate " + Guid.NewGuid()
                                });
        }


        [Test]
        public void when_tariff_updated_then_dto_updated()
        {
            this.sut.Handle(new TariffUpdated
            {
                SourceId = _companyId,
                TariffId = _tariffId,
                DaysOfTheWeek = DayOfTheWeek.Tuesday,
                KilometricRate = 19,
                FlatRate = 20,
                PassengerRate = 21,
                MarginOfError = 22,
                StartTime = DateTime.Today.AddHours(23),
                EndTime = DateTime.Today.AddHours(24),
                Name = "Updated Rate",
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<TariffDetail>().Where(x => x.Id == _tariffId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(_tariffId, dto.Id);
                Assert.AreEqual(_companyId, dto.CompanyId);
                Assert.AreEqual(20, dto.FlatRate);
                Assert.AreEqual(19, dto.KilometricRate);
                Assert.AreEqual(22, dto.MarginOfError);
                Assert.AreEqual(21, dto.PassengerRate);
                Assert.AreEqual((int)(DayOfTheWeek.Tuesday), dto.DaysOfTheWeek);
                Assert.AreEqual(23, dto.StartTime.Hour);
                Assert.AreEqual(0, dto.EndTime.Hour);
            }
        }

        [Test]
        public void when_tariff_deleted()
        {
            this.sut.Handle(new TariffDeleted
            {
                SourceId = _companyId,
                TariffId = _tariffId
            });

            using (var context = new BookingDbContext(dbName))
            {
                Assert.IsFalse(context.Query<TariffDetail>().Any(x => x.Id == _tariffId));
            }
        }
    }
}
