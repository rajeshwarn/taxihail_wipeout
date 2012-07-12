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
        
        }

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Order>();
            this.sut.Setup(new OrderCommandHandler(this.sut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, Name = "Bob", Password = null, Email = "bob.smith@apcurium.com" });            
        }


        [Test]
        public void when_creating_an_order_successfully()
        {
            var pickupDate = DateTime.Now;
            var requestDate = DateTime.Now.AddHours(1);
            var order = new CreateOrder
            {
                AccountId = _accountId,
                PickupDate = pickupDate,
                PickupAddress = new CreateOrder.Address { RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064, FullAddress = "1234 rue Saint-Hubert", Apartment = "3939"},
                DropOffAddress = new CreateOrder.Address { Latitude = 45.50643, Longitude = -73.554052, FullAddress = "Velvet auberge st gabriel" },           
                IBSOrderId = 99,
            };
            order.Settings = new CreateOrder.BookingSettings { ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            this.sut.When(order );

            Assert.AreEqual(1, sut.Events.Count);
            var orderCreated = (OrderCreated) sut.Events.First();
            Assert.AreEqual(_accountId, orderCreated.AccountId);
            Assert.AreEqual(pickupDate, orderCreated.PickupDate);                        
            Assert.AreEqual("3939", orderCreated.PickupApartment);
            Assert.AreEqual("1234 rue Saint-Hubert", orderCreated.PickupAddress);
            Assert.AreEqual("3131", orderCreated.PickupRingCode);
            Assert.AreEqual(45.515065, orderCreated.PickupLatitude);
            Assert.AreEqual(-73.558064, orderCreated.PickupLongitude);
            Assert.AreEqual("Velvet auberge st gabriel", orderCreated.DropOffAddress);
            Assert.AreEqual(45.50643, orderCreated.DropOffLatitude);
            Assert.AreEqual(99, orderCreated.IBSOrderId);
            Assert.AreEqual(-73.554052, orderCreated.DropOffLongitude);
            
        }


        [Test]
        public void when_creating_an_order_without_dropOff_successfully()
        {
            var pickupDate = DateTime.Now;
            var requestDate = DateTime.Now.AddHours(1);


            var order = new CreateOrder
            {
                AccountId = _accountId,
                PickupDate = pickupDate,
                PickupAddress = new CreateOrder.Address { RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064, FullAddress = "1234 rue Saint-Hubert", Apartment="3939" },
                IBSOrderId = 99,
            };
            order.Settings = new CreateOrder.BookingSettings { ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" };

            this.sut.When(order);



            Assert.AreEqual(1, sut.Events.Count);
            var orderCreated = (OrderCreated)sut.Events.First();
            Assert.AreEqual(_accountId, orderCreated.AccountId);
            Assert.AreEqual(pickupDate, orderCreated.PickupDate);
            Assert.AreEqual("3939", orderCreated.PickupApartment);
            Assert.AreEqual("1234 rue Saint-Hubert", orderCreated.PickupAddress);
            Assert.AreEqual("3131", orderCreated.PickupRingCode);
            Assert.AreEqual(45.515065, orderCreated.PickupLatitude);
            Assert.AreEqual(-73.558064, orderCreated.PickupLongitude);
            Assert.AreEqual(99, orderCreated.IBSOrderId);            
            Assert.IsNull(orderCreated.DropOffAddress);
            Assert.IsNull(orderCreated.DropOffLatitude);
            Assert.IsNull(orderCreated.DropOffLongitude);

        }
    }
}
