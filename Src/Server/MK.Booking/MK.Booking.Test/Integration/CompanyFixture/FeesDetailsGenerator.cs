using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.CompanyFixture
{
    public class given_a_fees_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected FeesDetailsGenerator Sut;

        public given_a_fees_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new FeesDetailsGenerator(new EntityProjectionSet<FeesDetail>(() => new BookingDbContext(DbName)));
        }
    }

    [TestFixture]
    public class given_no_fee_structure : given_a_fees_generator
    {
        [Test]
        public void when_a_fee_structure_is_created_then_dtos_created()
        {
            var fees = new List<Fees>
            {
                new Fees {Market = null, Booking = 1, Cancellation = 2, NoShow = 2},
                new Fees {Market = "MTL", Booking = 0, Cancellation = 0, NoShow = 3}
            };

            var @event = new FeesUpdated
            {
                SourceId = Guid.NewGuid(),
                Fees = fees
            };

            Sut.Handle(@event);

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<FeesDetail>().ToList();
                Assert.AreEqual(2, list.Count());

                var localFees = list.Single(x => x.Market == null);
                Assert.AreEqual(@event.Fees.First().Market, localFees.Market);
                Assert.AreEqual(@event.Fees.First().Booking, localFees.Booking);
                Assert.AreEqual(@event.Fees.First().Cancellation, localFees.Cancellation);
                Assert.AreEqual(@event.Fees.First().NoShow, localFees.NoShow);

                var mtlFees = list.Single(x => x.Market == "MTL");
                Assert.AreEqual(@event.Fees.Skip(1).First().Market, mtlFees.Market);
                Assert.AreEqual(@event.Fees.Skip(1).First().Booking, mtlFees.Booking);
                Assert.AreEqual(@event.Fees.Skip(1).First().Cancellation, mtlFees.Cancellation);
                Assert.AreEqual(@event.Fees.Skip(1).First().NoShow, mtlFees.NoShow);
            }
        }
    }
}