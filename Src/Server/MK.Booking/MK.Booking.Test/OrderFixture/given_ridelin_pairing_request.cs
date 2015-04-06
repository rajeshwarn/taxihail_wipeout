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
            var rideLinqPairing = new CreateOrderForManualRideLinqPair
            {
                AccountId = _accountId,
                ClientLanguageCode = "fr",
                UserAgent = "TestUserAgent",
                ClientVersion = "1.0.0",
                RideLinQId = "0000000",
                StartTime = pickupDate
            };

            _sut.When(rideLinqPairing);
            
            var rideLinqPaired = _sut.ThenHasSingle<ManualRideLinqPaired>();

            Assert.AreEqual(_accountId, rideLinqPaired.AccountId);
            Assert.AreEqual(pickupDate, rideLinqPaired.StartTime);
        }
    }
}
