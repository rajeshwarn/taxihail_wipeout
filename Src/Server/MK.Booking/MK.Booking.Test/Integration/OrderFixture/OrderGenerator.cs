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

namespace apcurium.MK.Booking.Test.Integration.OrderFixture
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected OrderGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new OrderGenerator(() => new BookingDbContext(dbName));
        }
    }

    [TestFixture]
    public class given_no_account : given_a_view_model_generator
    {
        [Test]
        public void when_order_created_then_order_dto_populated()
        {
            var orderId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var pickupDate = DateTime.Now;
            var requestDate = DateTime.Now.AddHours(1);
            this.sut.Handle(new OrderCreated
            {
                SourceId = orderId,
                AccountId = accountId,
                FriendlyName = "Chez François",
                Apartment = "3939",
                FullAddress = "1234 rue Saint-Hubert",
                RingCode = "3131",
                Latitude = 45.515065,
                Longitude = -73.558064,
                PickupDate =  pickupDate,
                RequestedDateTime = requestDate,
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<OrderDetail>().Where(x => x.Id == orderId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual("Chez François", dto.FriendlyName);
                Assert.AreEqual("3939", dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.AreEqual("3131", dto.RingCode);
                Assert.AreEqual(45.515065, dto.Latitude);
                Assert.AreEqual(-73.558064, dto.Longitude);
                Assert.AreEqual(pickupDate.ToLongDateString(), dto.PickupDate.ToLongDateString());
                Assert.AreEqual(requestDate.ToLongDateString(), dto.RequestedDateTime.ToLongDateString());
            }
        }
    }
}
