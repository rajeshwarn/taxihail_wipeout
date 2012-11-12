﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    [TestFixture]
    public class given_one_order
    {

        private EventSourcingTestHelper<Order> sut;
        private Guid _orderId = Guid.NewGuid();
        private Guid _accountId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Order>();
            this.sut.Setup(new OrderCommandHandler(this.sut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, Name = "Bob", Password = null, Email = "bob.smith@apcurium.com" });
            this.sut.Given(new OrderCreated { SourceId = _orderId, AccountId = Guid.NewGuid(), PickupDate = DateTime.Now, 
                PickupAddress = new Address
                                    {
                                        FullAddress = "1234 rue Saint-Hubert",
                                        Apartment = "3939",
                                        RingCode = "3131",
                                        Latitude = 45.515065,
                                        Longitude = -73.558064,
                                    }});
        }

        [Test]
        public void when_cancelling_successfully()
        {
            this.sut.When(new CancelOrder { OrderId = _orderId });
            
            var @event = sut.ThenHasSingle<OrderCancelled>();            
            Assert.AreEqual(_orderId, @event.SourceId);
        }

        [Test]
        public void when_complete_order_successfully()
        {
            var completeOrder = new CompleteOrder {OrderId = _orderId, Date = DateTime.Now, Fare = 23, Toll = 2, Tip = 5};
            this.sut.When(completeOrder);

            var @event = sut.ThenHasSingle<OrderCompleted>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.That(@event.Date, Is.EqualTo(completeOrder.Date).Within(1).Seconds);
            Assert.AreEqual(completeOrder.Fare, @event.Fare);
            Assert.AreEqual(completeOrder.Toll, @event.Toll);
            Assert.AreEqual(completeOrder.Tip, @event.Tip);
        }

        [Test]
        public void when_complete_twice_order_one_event_only()
        {
            this.sut.Given(new OrderCompleted(){ SourceId =  _orderId});
            this.sut.When(new CompleteOrder { OrderId = _orderId });

            Assert.AreEqual(0,sut.Events.Count);
        }

        [Test]
        public void when_remove_from_history_successfully()
        {
            this.sut.When(new RemoveOrderFromHistory() { OrderId = _orderId });

            sut.ThenHasSingle<OrderRemovedFromHistory>();
        }


        [Test]
        public void when_rating_order_successfully()
        {
            var rateOrder = new RateOrder
                                {
                                    OrderId = _orderId,
                                    Note = "Note",
                                    RatingScores = new List<RatingScore>
                                                       {
                                                           new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1, Name = "Politness"},
                                                           new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"}
                                                       }
                                };

            this.sut.When(rateOrder);

            var @event = sut.ThenHasSingle<OrderRated>();
            Assert.AreEqual(_orderId, @event.SourceId);
            Assert.That(@event.Note, Is.EqualTo(rateOrder.Note));
            Assert.That(@event.RatingScores.Count, Is.EqualTo(2));
            Assert.That(@event.RatingScores.First().Score, Is.EqualTo(rateOrder.RatingScores.First().Score));
            Assert.That(@event.RatingScores.First().RatingTypeId, Is.EqualTo(rateOrder.RatingScores.First().RatingTypeId));
            Assert.That(@event.RatingScores.First().Name, Is.EqualTo(rateOrder.RatingScores.First().Name));
            Assert.That(@event.RatingScores.ElementAt(1).Score, Is.EqualTo(rateOrder.RatingScores.ElementAt(1).Score));
            Assert.That(@event.RatingScores.ElementAt(1).RatingTypeId, Is.EqualTo(rateOrder.RatingScores.ElementAt(1).RatingTypeId));
            Assert.That(@event.RatingScores.ElementAt(1).Name, Is.EqualTo(rateOrder.RatingScores.ElementAt(1).Name));
        }

        [Test]
        public void when_rating_twice_order_get_error()
        {
            var orderRated = new OrderRated()
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
                OrderId = _orderId,
                Note = "Note",
                RatingScores = new List<RatingScore>
                                                       {
                                                           new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1, Name = "Politness"},
                                                           new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"}
                                                       }
            };

            this.sut.Given(orderRated);
            this.sut.When(rateOrder2);
            Assert.AreEqual(0, sut.Events.Count);

        }
    }
}
