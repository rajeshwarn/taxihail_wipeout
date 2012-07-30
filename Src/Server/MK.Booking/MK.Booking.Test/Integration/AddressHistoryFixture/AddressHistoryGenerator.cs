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

namespace apcurium.MK.Booking.Test.Integration.AddressHistoryFixture
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected AddressHistoryGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new AddressHistoryGenerator(() => new BookingDbContext(dbName));
        }
    }
    [TestFixture]
    public class given_an_address : given_a_view_model_generator
    {
        [Test]
        public void when_address_is_added_then_address_removed_from_history()
        {
            var addressId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var pickupDate = DateTime.Now;
            var requestedDate = DateTime.Now.AddDays(-1);
            this.sut.Handle(new OrderCreated()
                                {
                                    AccountId = accountId,
                                    PickupApartment = "3939",
                                    PickupRingCode = "3131",
                                    PickupAddress = "1234 rue Saint-Denis",
                                    PickupLongitude = -73.558064,
                                    PickupLatitude = 45.515065,

                                });
            this.sut.Handle(new AddressAdded()
                                {
                                    SourceId = accountId,
                                    Apartment = "3939",
                                    RingCode = "3131",
                                    FriendlyName = "La Boite à Jojo",
                                    FullAddress = "1234 rue Saint-Denis",
                                    Longitude = -73.558064,
                                    Latitude = 45.515065,
                                    AddressId = addressId
                                });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<Address>().Where(x => x.AccountId == accountId && x.FullAddress.Equals("1234 rue Saint-Denis"));
                Assert.AreEqual(0, list.Count());
            }
        }
    }

    [TestFixture]
    public class given_an_order : given_a_view_model_generator
    {
        [Test]
        public void when_order_created_pickup_then_address_is_added_to_history()
        {
            var orderId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var pickupDate = DateTime.Now.AddDays(1);
            var createdDate = DateTime.Now;
            this.sut.Handle(new OrderCreated
            {
                SourceId = orderId,
                AccountId = accountId,

                PickupApartment = "3939",
                PickupAddress = "1234 rue Saint-Hubert",
                PickupRingCode = "3131",
                PickupLatitude = 45.515065,
                PickupLongitude = -73.558064,
                PickupDate = pickupDate,
                DropOffAddress = "Velvet auberge st gabriel",
                DropOffLatitude = 45.50643,
                DropOffLongitude = -73.554052,
                CreatedDate = createdDate
            });

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<Address>().Where(x => x.AccountId == accountId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual("3939", dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.AreEqual("3131", dto.RingCode);
                Assert.AreEqual(45.515065, dto.Latitude);
                Assert.AreEqual(-73.558064, dto.Longitude);
            }
        }

        [Test]
        public void when_order_created_no_duplicate_address_is_added_to_history()
        {
            var command = new OrderCreated
            {
                SourceId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                PickupApartment = "3939",
                PickupAddress = "1234 rue Saint-Hubert",
                PickupRingCode = "3131",
                PickupLatitude = 45.515065,
                PickupLongitude = -73.558064,
                PickupDate = DateTime.Now,
                DropOffAddress = "Velvet auberge st gabriel",
                DropOffLatitude = 45.50643,
                DropOffLongitude = -73.554052,
                CreatedDate = DateTime.Now.AddDays(-1)
            };

            // Use the same address twice
            this.sut.Handle(command);
            this.sut.Handle(command);

            using (var context = new BookingDbContext(dbName))
            {
                var list = context.Query<Address>().Where(x => x.AccountId == command.AccountId && x.IsHistoric.Equals(true));
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(command.AccountId, dto.AccountId);
                Assert.AreEqual("3939", dto.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.FullAddress);
                Assert.AreEqual("3131", dto.RingCode);
                Assert.AreEqual(45.515065, dto.Latitude);
                Assert.AreEqual(-73.558064, dto.Longitude);
            }
        }
    }
}
