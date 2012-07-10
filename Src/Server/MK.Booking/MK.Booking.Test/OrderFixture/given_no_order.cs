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

namespace apcurium.MK.Booking.Test.OrderFixture
{
    [TestFixture]
    public class given_no_order
    {
        private EventSourcingTestHelper<Order> sut;
        private readonly Guid _accountId = Guid.NewGuid();

        public given_no_order()
        {
            this.sut = new EventSourcingTestHelper<Order>();
            this.sut.Setup(new OrderCommandHandler(this.sut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, FirstName = "Bob", LastName = "Smith", Password = null, Email = "bob.smith@apcurium.com" });
            //this.sut.Given(new OrderCreated { SourceId = _orderId, AccountId = Guid.NewGuid(), PickupDate = DateTime.Now, RequestedDateTime = DateTime.Now.AddHours(1), FriendlyName = "Chez François", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 });
        }

        [Test]
        public void when_creating_an_order_successfully()
        {
            var pickupDate = DateTime.Now;
            var requestDate = DateTime.Now.AddHours(1);
            this.sut.When(new CreateOrder
            {
                AccountId = _accountId,
                PickupDate = pickupDate,
                PickupApartment = "3939",
                PickupAddress = "1234 rue Saint-Hubert",
                PickupRingCode = "3131",
                PickupLatitude = 45.515065,
                PickupLongitude = -73.558064,
                DropOffAddress = "Velvet auberge st gabriel",
                DropOffLatitude = 45.50643,
                DropOffLongitude = -73.554052,
            });

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (OrderCreated)sut.Events.Single();
            Assert.AreEqual(_accountId, evt.AccountId);
            Assert.AreEqual(pickupDate, evt.PickupDate);                        
            Assert.AreEqual("3939", evt.PickupApartment);
            Assert.AreEqual("1234 rue Saint-Hubert", evt.PickupAddress);
            Assert.AreEqual("3131", evt.PickupRingCode);
            Assert.AreEqual(45.515065, evt.PickupLatitude);
            Assert.AreEqual(-73.558064, evt.PickupLongitude);
            Assert.AreEqual("Velvet auberge st gabriel", evt.DropOffAddress);
            Assert.AreEqual(45.50643, evt.DropOffLatitude);
            Assert.AreEqual(-73.554052, evt.DropOffLongitude);

        }
    }
}
