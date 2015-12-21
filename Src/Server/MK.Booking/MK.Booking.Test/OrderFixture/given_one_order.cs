using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Test.Integration;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    [TestFixture]
    public class given_one_order : given_a_read_model_database
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Order>();
            _sut.Setup(new OrderCommandHandler(_sut.Repository));
            _sut.Given(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob",
                Password = null,
                Email = "bob.smith@apcurium.com"
            });
            _sut.Given(new OrderCreated
            {
                SourceId = _orderId,
                AccountId = Guid.NewGuid(),
                PickupDate = DateTime.Now,
                PickupAddress = new Address
                {
                    FullAddress = "1234 rue Saint-Hubert",
                    Apartment = "3939",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064,
                }
            });
        }

        private EventSourcingTestHelper<Order> _sut;
        private readonly Guid _orderId = Guid.NewGuid();
        private readonly Guid _accountId = Guid.NewGuid();

        [Test]
        public void when_adding_ibs_order_info_to_order()
        {
            _sut.When(new AddIbsOrderInfoToOrder { OrderId = _orderId, IBSOrderId = 99, CompanyKey = null });

            var @event = _sut.ThenHasSingle<IbsOrderInfoAddedToOrder_V2>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual(null, @event.CompanyKey);
            Assert.AreEqual(99, @event.IBSOrderId);
        }

        [Test]
        public void when_adding_ibs_order_info_to_order_with_different_companyKey()
        {
            _sut.When(new AddIbsOrderInfoToOrder { OrderId = _orderId, IBSOrderId = 99, CompanyKey = "key" });

            var @event = _sut.ThenHasSingle<IbsOrderInfoAddedToOrder_V2>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual("key", @event.CompanyKey);
            Assert.AreEqual(99, @event.IBSOrderId);
            Assert.AreEqual(false, @event.CancelWasRequested);
        }

        [Test]
        public void when_adding_ibs_order_info_to_order_and_order_was_cancelled_beforehand()
        {
            _sut.Given(new OrderCancelled { SourceId = _orderId });
            _sut.When(new AddIbsOrderInfoToOrder { OrderId = _orderId, IBSOrderId = 99 });

            var @event = _sut.ThenHasSingle<IbsOrderInfoAddedToOrder_V2>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual(99, @event.IBSOrderId);
            Assert.AreEqual(true, @event.CancelWasRequested);
        }

        [Test]
        public void when_cancelling_successfully()
        {
            _sut.When(new CancelOrder {OrderId = _orderId});

            var @event = _sut.ThenHasSingle<OrderCancelled>();
            Assert.AreEqual(_orderId, @event.SourceId);
        }

        [Test]
        public void when_complete_order_successfully()
        {
            var completeOrder = new ChangeOrderStatus
            {
                Status = new OrderStatusDetail {OrderId = _orderId, Status = OrderStatus.Completed},
                Fare = 23,
                Toll = 2,
                Tip = 5,
                Tax = 3,
                Surcharge = 1
            };
            _sut.When(completeOrder);

            var @event = _sut.ThenHasOne<OrderStatusChanged>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual(completeOrder.Fare, @event.Fare);
            Assert.AreEqual(completeOrder.Toll, @event.Toll);
            Assert.AreEqual(completeOrder.Tip, @event.Tip);
            Assert.AreEqual(completeOrder.Tax, @event.Tax);
            Assert.AreEqual(completeOrder.Surcharge, @event.Surcharge);
            Assert.AreEqual(true, @event.IsCompleted);
        }

        [Test]
        public void when_change_status_with_same_status_twice_no_event_published()
        {
            _sut.Given(new OrderStatusChanged { SourceId = _orderId, IsCompleted = true, Status = new OrderStatusDetail { OrderId = _orderId, Status = OrderStatus.Completed, IBSStatusId = VehicleStatuses.Common.Done } });
            _sut.When(new ChangeOrderStatus { Status = new OrderStatusDetail { OrderId = _orderId, Status = OrderStatus.Completed, IBSStatusId = VehicleStatuses.Common.Done } });

            Assert.AreEqual(false, _sut.ThenContains<OrderStatusChanged>());
            Assert.AreEqual(0, _sut.Events.Count);
        }

        [Test]
        public void when_change_status_with_same_status_twice_but_different_fare_then_event_published()
        {
            _sut.Given(new OrderStatusChanged { SourceId = _orderId, IsCompleted = true, Status = new OrderStatusDetail { OrderId = _orderId, Status = OrderStatus.Completed, IBSStatusId = VehicleStatuses.Common.Done } });
            _sut.When(new ChangeOrderStatus { Status = new OrderStatusDetail { OrderId = _orderId, Status = OrderStatus.Completed, IBSStatusId = VehicleStatuses.Common.Done }, Fare = 12 });

            var @event = _sut.ThenHasOne<OrderStatusChanged>();
            Assert.AreEqual(12, @event.Fare);
            Assert.AreEqual(1, _sut.Events.Count);
        }

        [Test]
        public void when_ibs_status_changed()
        {
            _sut.When(new ChangeOrderStatus
            {
                Status = new OrderStatusDetail
                {
                    OrderId = _orderId,
                    IBSStatusId = Guid.NewGuid().ToString()
                }
            });

            _sut.ThenHasSingle<OrderStatusChanged>();
        }

        [Test]
        public void when_ibs_status_has_not_changed_then_no_event()
        {
            var status = new OrderStatusDetail
            {
                OrderId = _orderId,
                IBSStatusId = Guid.NewGuid().ToString()
            };

            _sut.Given(new OrderStatusChanged
            {
                SourceId = _orderId,
                Status = status
            });

            _sut.When(new ChangeOrderStatus
            {
                Status = status,
            });

            Assert.AreEqual(0, _sut.Events.Count);
        }

        [Test]
        public void when_ibs_fare_changed()
        {
            var status = new OrderStatusDetail
            {
                OrderId = _orderId
            };

            _sut.When(new ChangeOrderStatus
            {
                Status = status,
                Fare = 12
            });

            var @event = _sut.ThenHasSingle<OrderStatusChanged>();

            Assert.AreEqual(12, @event.Fare);
        }

        [Test]
        public void when_rating_order_successfully()
        {
            var rateOrder = new RateOrder
            {
				AccountId = _accountId,
                OrderId = _orderId,
                Note = "Note",
                RatingScores = new List<RatingScore>
                {
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1, Name = "Politness"},
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"}
                }
            };

            _sut.When(rateOrder);

            var @event = _sut.ThenHasSingle<OrderRated>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.That(@event.Note, Is.EqualTo(rateOrder.Note));
            Assert.That(@event.RatingScores.Count, Is.EqualTo(2));
            Assert.That(@event.RatingScores.First().Score, Is.EqualTo(rateOrder.RatingScores.First().Score));
            Assert.That(@event.RatingScores.First().RatingTypeId,
                Is.EqualTo(rateOrder.RatingScores.First().RatingTypeId));
            Assert.That(@event.RatingScores.First().Name, Is.EqualTo(rateOrder.RatingScores.First().Name));
            Assert.That(@event.RatingScores.ElementAt(1).Score, Is.EqualTo(rateOrder.RatingScores.ElementAt(1).Score));
            Assert.That(@event.RatingScores.ElementAt(1).RatingTypeId,
                Is.EqualTo(rateOrder.RatingScores.ElementAt(1).RatingTypeId));
            Assert.That(@event.RatingScores.ElementAt(1).Name, Is.EqualTo(rateOrder.RatingScores.ElementAt(1).Name));
        }

        [Test]
        public void when_rating_twice_order_get_error()
        {
            var orderRated = new OrderRated
            {
                SourceId = _orderId,
                Note = "Note",
                RatingScores = new List<RatingScore>
                {
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1, Name = "Politness"},
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"}
                }
            };

            var rateOrder2 = new RateOrder
            {
				AccountId = _accountId,
                OrderId = _orderId,
                Note = "Note",
                RatingScores = new List<RatingScore>
                {
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1, Name = "Politness"},
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"}
                }
            };

            _sut.Given(orderRated);
            _sut.When(rateOrder2);
            Assert.AreEqual(0, _sut.Events.Count);
        }

        [Test]
        public void when_remove_from_history_successfully()
        {
            _sut.When(new RemoveOrderFromHistory {OrderId = _orderId});

            _sut.ThenHasSingle<OrderRemovedFromHistory>();
        }

        [Test]
        public void when_order_timed_out()
        {
            _sut.When(new NotifyOrderTimedOut
            {
                OrderId = _orderId
            });

            _sut.ThenHasSingle<OrderTimedOut>();
        }

        [Test]
        public void when_order_prepared_for_next_dispatch()
        {
            _sut.When(new PrepareOrderForNextDispatch
            {
                OrderId = _orderId,
                DispatchCompanyName = "Kukai Foundation",
                DispatchCompanyKey = "123456"
            });

            var @event = _sut.ThenHasSingle<OrderPreparedForNextDispatch>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual("Kukai Foundation", @event.DispatchCompanyName);
            Assert.AreEqual("123456", @event.DispatchCompanyKey);
        }

        [Test]
        public void when_order_switched_to_next_dispatch_company()
        {
            _sut.When(new SwitchOrderToNextDispatchCompany
            {
                OrderId = _orderId,
                IBSOrderId = 67865,
                CompanyKey = "x2s42",
                CompanyName = "Vector Industries"
            });

            var @event = _sut.ThenHasSingle<OrderSwitchedToNextDispatchCompany>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.AreEqual(67865, @event.IBSOrderId);
            Assert.AreEqual("x2s42", @event.CompanyKey);
            Assert.AreEqual("Vector Industries", @event.CompanyName);
        }

        [Test]
        public void when_dispatch_company_switch_ignored()
        {
            _sut.When(new IgnoreDispatchCompanySwitch
            {
                OrderId = _orderId
            });

            var @event = _sut.ThenHasSingle<DispatchCompanySwitchIgnored>();
            Assert.AreEqual(_orderId, @event.SourceId);
        }
    }
}