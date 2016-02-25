using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Test.Integration;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    [TestFixture]
    public class given_one_order_created_by_ridelinq_pairing : given_a_read_model_database
    {
        private EventSourcingTestHelper<Order> _sut;
        private readonly Guid _accountId = Guid.NewGuid();
        private readonly Guid _orderId = Guid.NewGuid();

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
            _sut.Given(new OrderManuallyPairedForRideLinq()
            {
                AccountId = _accountId,
                SourceId = _orderId
            });
        }

        [Test]
        public void when_ridelinq_update_trip_info()
        {
            _sut.When(new UpdateTripInfoInOrderForManualRideLinq
            {
                OrderId = _orderId,
                Distance = 25d,
                Fare = 15f,
                Tax = 3f,
                TollTotal = 1f,
                Extra = .5f,
                Tip= 1.5f
            });

            var @event = _sut.ThenHasOne<ManualRideLinqTripInfoUpdated>();

            Assert.AreEqual(15f, @event.Fare);
            Assert.AreEqual(3f, @event.Tax );
            Assert.AreEqual(1.5f, @event.Tip);
            Assert.AreEqual(25d, @event.Distance);
            Assert.AreEqual(1f, @event.Toll);
            Assert.AreEqual(.5f, @event.Extra);
            Assert.IsNull(@event.EndTime);
        }

        [Test]
        public void when_ridelinq_order_unpair()
        {
            _sut.When(new UnpairOrderForManualRideLinq
            {
                OrderId = _orderId
            });

            var @event = _sut.ThenHasOne<OrderUnpairedFromManualRideLinq>();
            Assert.AreEqual(_orderId, @event.SourceId);
        }

        [Test]
        public void when_ridelinq_order_completes()
        {
            var endTime = DateTime.Now;

            _sut.When(new UpdateTripInfoInOrderForManualRideLinq
            {
                OrderId = _orderId,
                Distance = 25d,
                Fare = 15f,
                Tax = 3f,
                Tip = 1.5f,
                TollTotal = 1f,
                Extra = .5f,
                EndTime = endTime
            });

            var @event = _sut.ThenHasOne<ManualRideLinqTripInfoUpdated>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual(15f, @event.Fare);
            Assert.AreEqual(3f, @event.Tax);
            Assert.AreEqual(1.5f, @event.Tip);
            Assert.AreEqual(25d, @event.Distance);
            Assert.AreEqual(endTime, @event.EndTime);
        }

        [Test]
        public void when_ridelinq_order_is_still_active_after_two_hours()
        {
            var date = DateTime.UtcNow;

            _sut.When(new ChangeOrderStatusForManualRideLinq
            {
                OrderId = _orderId,
                Status = OrderStatus.WaitingForPayment,
                LastTripPollingDateInUtc = date
            });

            var @event = _sut.ThenHasOne<OrderStatusChangedForManualRideLinq>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual(OrderStatus.WaitingForPayment, @event.Status);
            Assert.AreEqual(date, @event.LastTripPollingDateInUtc);
        }

        [Test]
        public void when_ridelinq_order_is_still_active_after_30_days()
        {
            var date = DateTime.UtcNow;

            _sut.Given(new OrderStatusChangedForManualRideLinq
            {
                Status = OrderStatus.WaitingForPayment,
                SourceId = _orderId,
                LastTripPollingDateInUtc = date.AddHours(-4)
            });

            _sut.When(new ChangeOrderStatusForManualRideLinq
            {
                OrderId = _orderId,
                Status = OrderStatus.TimedOut,
                LastTripPollingDateInUtc = date
            });

            var @event = _sut.ThenHasOne<OrderStatusChangedForManualRideLinq>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual(OrderStatus.TimedOut, @event.Status);
            Assert.AreEqual(date, @event.LastTripPollingDateInUtc);
        }
    }
}
