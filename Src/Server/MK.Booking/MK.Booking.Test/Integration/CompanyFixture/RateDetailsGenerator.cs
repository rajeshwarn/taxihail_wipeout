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
        protected RateDetailsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new RateDetailsGenerator(() => new BookingDbContext(dbName));
        }
    }

    [TestFixture]
    public class given_no_rate : given_a_view_model_generator
    {
        [Test]
        public void when_rate_created_rate_dto_populated()
        {
            var rateId = Guid.NewGuid();
            var companyId = Guid.NewGuid();
            this.sut.Handle(new RateCreated
            {
                SourceId = companyId,
                RateId = rateId,
                FlatRate = 3.50m,
                DistanceMultiplicator = 1.1,
                TimeAdjustmentFactor = 1.2,
                PricePerPassenger = 2.0m,
                DaysOfTheWeek = DayOfTheWeek.Saturday | DayOfTheWeek.Sunday
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<RateDetail>().Where(x => x.Id == rateId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(rateId, dto.Id);
                Assert.AreEqual(companyId, dto.CompanyId);
                Assert.AreEqual(3.50m, dto.FlatRate);
                Assert.AreEqual(1.1, dto.DistanceMultiplicator);
                Assert.AreEqual(1.2, dto.TimeAdjustmentFactor);
                Assert.AreEqual(2.0m, dto.PricePerPassenger);
                Assert.AreEqual((int)(DayOfTheWeek.Saturday | DayOfTheWeek.Sunday), dto.DaysOfTheWeek);
            }
        }
    }
}
