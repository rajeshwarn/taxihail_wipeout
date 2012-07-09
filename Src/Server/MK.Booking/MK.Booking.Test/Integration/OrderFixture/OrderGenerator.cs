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
    public class given_no_order : given_a_view_model_generator
    {
        [Test]
        public void when_order_created_then_order_dto_populated()
        {
            var orderId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var pickupDate = DateTime.Now;
            var requestedDate = DateTime.Now.AddDays(-1);
            this.sut.Handle(new OrderCreated
            {
                SourceId = orderId,
                AccountId = accountId,
                
                PickupApartment = "3939",
                PickupAddress = "1234 rue Saint-Hubert",
                PickupRingCode = "3131",
                PickupLatitude = 45.515065,
                PickupLongitude = -73.558064,
                PickupDate =  pickupDate,
                DropOffAddress = "Velvet auberge st gabriel",
                DropOffLatitude = 45.50643,
                DropOffLongitude = -73.554052,
                RequestedDate = requestedDate
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<OrderDetail>().Where(x => x.Id == orderId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(accountId, dto.AccountId);                
                Assert.AreEqual("3939", dto.PickupApartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.PickupAddress);
                Assert.AreEqual("3131", dto.PickupRingCode);
                Assert.AreEqual(45.515065, dto.PickupLatitude);
                Assert.AreEqual(-73.558064, dto.PickupLongitude);
                Assert.AreEqual("Velvet auberge st gabriel", dto.DropOffAddress);
                Assert.AreEqual(45.50643, dto.DropOffLatitude);
                Assert.AreEqual(-73.554052, dto.DropOffLongitude);
                Assert.AreEqual(pickupDate.ToLongDateString(), dto.PickupDate.ToLongDateString());                
            }
        }
    }

    [TestFixture]
    public class given_existing_order : given_a_view_model_generator
    {
        private Guid _orderId = Guid.NewGuid();
        private Guid _accountId = Guid.NewGuid();

        public given_existing_order()
        {
            var pickupDate = DateTime.Now;
            var requestDate = DateTime.Now.AddHours(1);

            this.sut.Handle(new OrderCreated()
                                {
                                    SourceId = _orderId,
                                    AccountId = _accountId,
                                    
                                    PickupApartment = "3939",
                                    PickupAddress = "1234 rue Saint-Hubert",
                                    PickupRingCode = "3131",
                                    PickupLatitude = 45.515065,
                                    PickupLongitude = -73.558064,
                                    PickupDate = pickupDate,
                                    DropOffAddress = "Velvet auberge st gabriel",
                                    DropOffLatitude = 45.50643,
                                    DropOffLongitude = -73.554052,
                                    RequestedDate = DateTime.Now.AddDays(-1),
                                });

        }

        [Test]
        public void when_order_cancelled_then_order_dto_populated()
        {
            this.sut.Handle(new OrderCancelled()
                                {
                                    SourceId = _orderId,

                                });

            using (var context = new BookingDbContext(dbName))
            {
                var dto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(dto);
            }
        }
    }

}
