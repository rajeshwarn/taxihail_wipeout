using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Test.Integration;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    [TestFixture]
    public class given_ridelin_pairing_request : given_a_read_model_database
    {
        private EventSourcingTestHelper<Order> _sut;
        private readonly Guid _accountId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Order>();
            _sut.Setup(new OrderCommandHandler(_sut.Repository, () => new BookingDbContext(DbName)));
            _sut.Given(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob",
                Password = null,
                Email = "bob.smith@apcurium.com"
            });
        }

        [Test]
        public void when_creating_an_order_successfully()
        {
            var pickupDate = DateTime.Now;
            var orderId = Guid.NewGuid();

            var rideLinqPairing = new CreateOrderForManualRideLinqPair
            {
                AccountId = _accountId,
                ClientLanguageCode = "fr",
                UserAgent = "TestUserAgent",
                ClientVersion = "1.0.0",
                PairingCode = "0000000",
                PairingDate = pickupDate,
                PairingToken = "TESTTOKEN",
                Distance = 0f,
                OrderId = orderId,
                Medallion = "3",
				DeviceName = "EH1234",
                Fare = 12f,
                FareAtAlternateRate = 13f,
                RateAtTripEnd = 99f,
                RateAtTripStart = 3f,
                RateChangeTime = "10",
                Extra = 50f,
                Surcharge = 40f,
                Tax = 9f,
                Tip = 14f,
                Toll = 7f,
                Total = 5f
            };

            _sut.When(rideLinqPairing);
            
            var rideLinqPaired = _sut.ThenHasSingle<OrderManuallyPairedForRideLinq>();

            Assert.AreEqual(_accountId, rideLinqPaired.AccountId);
            Assert.AreEqual(pickupDate, rideLinqPaired.PairingDate);
            Assert.AreEqual(orderId, rideLinqPaired.SourceId);
            Assert.AreEqual(50f, rideLinqPaired.Extra);
            Assert.AreEqual("3", rideLinqPaired.Medallion);
			Assert.AreEqual("EH1234", rideLinqPaired.DeviceName);
            Assert.AreEqual(0f, rideLinqPaired.Distance);
            Assert.AreEqual(12f, rideLinqPaired.Fare);
            Assert.AreEqual(13f, rideLinqPaired.FareAtAlternateRate);
            Assert.AreEqual(99f, rideLinqPaired.RateAtTripEnd);
            Assert.AreEqual(3f, rideLinqPaired.RateAtTripStart);
            Assert.AreEqual("10", rideLinqPaired.RateChangeTime);
            Assert.AreEqual(40f, rideLinqPaired.Surcharge);
            Assert.AreEqual(9f, rideLinqPaired.Tax);
            Assert.AreEqual(14f, rideLinqPaired.Tip);
            Assert.AreEqual(7f, rideLinqPaired.Toll);
            Assert.AreEqual(5f, rideLinqPaired.Total);
        }
    }
}
