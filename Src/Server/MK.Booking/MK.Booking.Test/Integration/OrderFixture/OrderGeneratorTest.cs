#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Common.Tests;

#endregion

namespace apcurium.MK.Booking.Test.Integration.OrderFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected OrderGenerator Sut;

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new OrderGenerator(() => new BookingDbContext(DbName), new Logger(), new TestConfigurationManager());
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
            var pickupDate = DateTime.Now.AddDays(1);
            var createdDate = DateTime.Now;
            Sut.Handle(new OrderCreated
            {
                SourceId = orderId,
                AccountId = accountId,
                PickupAddress = new Address
                {
                    Apartment = "3939",
                    Street = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                },
                PickupDate = pickupDate,
                DropOffAddress = new Address
                {
                    FriendlyName = "Velvet auberge st gabriel",
                    Latitude = 45.50643,
                    Longitude = -73.554052,
                },
                Settings = new BookingSettings
                {
                    ChargeTypeId = 99,
                    VehicleTypeId = 98,
                    ProviderId = 97,
                    NumberOfTaxi = 96,
                    Passengers = 95,
                    Phone = "94",
                    Name = "93",
                    LargeBags = 92
                },
                CreatedDate = createdDate,
                ClientLanguageCode = "fr",
                UserAgent = "TestUserAgent",
                ClientVersion = "1.0.0"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var list = context.Query<OrderDetail>().Where(x => x.Id == orderId);
                Assert.AreEqual(1, list.Count());
                var dto = list.Single();
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual("3939", dto.PickupAddress.Apartment);
                Assert.AreEqual("1234 rue Saint-Hubert", dto.PickupAddress.Street);
                Assert.AreEqual("3131", dto.PickupAddress.RingCode);
                Assert.AreEqual(45.515065, dto.PickupAddress.Latitude);
                Assert.AreEqual(-73.558064, dto.PickupAddress.Longitude);
                Assert.AreEqual("Velvet auberge st gabriel", dto.DropOffAddress.FriendlyName);
                Assert.AreEqual(45.50643, dto.DropOffAddress.Latitude);
                Assert.AreEqual(-73.554052, dto.DropOffAddress.Longitude);
                Assert.AreEqual(pickupDate.ToLongDateString(), dto.PickupDate.ToLongDateString());
                Assert.AreEqual("fr", dto.ClientLanguageCode);
                Assert.AreEqual("TestUserAgent", dto.UserAgent);
                Assert.AreEqual("1.0.0", dto.ClientVersion);

                //Settings
                Assert.AreEqual(99, dto.Settings.ChargeTypeId);
                Assert.AreEqual(98, dto.Settings.VehicleTypeId);
                Assert.AreEqual(97, dto.Settings.ProviderId);
                Assert.AreEqual(96, dto.Settings.NumberOfTaxi);
                Assert.AreEqual(95, dto.Settings.Passengers);
                Assert.AreEqual("94", dto.Settings.Phone);
                Assert.AreEqual("93", dto.Settings.Name);
                Assert.AreEqual(92, dto.Settings.LargeBags);
            }
        }
    }

    [TestFixture]
    public class given_existing_order : given_a_view_model_generator
    {
        private readonly Guid _orderId = Guid.NewGuid();
        private readonly Guid _accountId = Guid.NewGuid();

        public given_existing_order()
        {
            var pickupDate = DateTime.Now;

            Sut.Handle(new OrderCreated
            {
                SourceId = _orderId,
                AccountId = _accountId,
                PickupAddress = new Address
                {
                    Apartment = "3939",
                    Street = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                },
                PickupDate = pickupDate,
                DropOffAddress = new Address
                {
                    Street = "Velvet auberge st gabriel",
                    Latitude = 45.50643,
                    Longitude = -73.554052,
                },
                CreatedDate = DateTime.Now,
            });
        }

        [Test]
        public void status_set_to_created()
        {
            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.NotNull(dto.Status == (int) OrderStatus.Created);
            }
        }

        [Test]
        public void when_cancelled_ibs_and_status_is_set()
        {
            Sut.Handle(new OrderCancelled {SourceId = _orderId});

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.AreEqual((int) OrderStatus.Canceled, dto.Status);

                var dtoDetails = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(dtoDetails);
                Assert.AreEqual(OrderStatus.Canceled, dtoDetails.Status);
                Assert.AreEqual(VehicleStatuses.Common.CancelledDone, dtoDetails.IBSStatusId);
            }
        }

        [Test]
        public void when_order_completed_then_order_dto_populated()
        {
            var orderCompleted = new OrderStatusChanged
            {
                SourceId = _orderId,
                Status = new OrderStatusDetail { Status = OrderStatus.Completed }
            };
            Sut.Handle(orderCompleted);

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.NotNull(dto.Status == (int) OrderStatus.Completed);
            }
        }


        [Test]
        public void when_order_rated_then_order_dto_populated()
        {
            var orderRated = new OrderRated
            {
                SourceId = _orderId,
                Note = "Note",
                RatingScores = new List<RatingScore>
                {
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1},
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2}
                }
            };
            Sut.Handle(orderRated);

            using (var context = new BookingDbContext(DbName))
            {
                var orderRatingDetailsDto =
                    context.Query<OrderRatingDetails>().SingleOrDefault(o => o.OrderId == _orderId);
                Assert.NotNull(orderRatingDetailsDto);
                Assert.That(orderRatingDetailsDto.Note, Is.EqualTo(orderRated.Note));

                var ratingScoreDetailsDto =
                    context.Query<RatingScoreDetails>().Where(s => s.OrderId == _orderId).ToList();
                Assert.That(ratingScoreDetailsDto.Count, Is.EqualTo(2));

                var x1 =
                    orderRated.RatingScores.Single(x => x.RatingTypeId == ratingScoreDetailsDto.First().RatingTypeId);
                Assert.That(ratingScoreDetailsDto.First().Score, Is.EqualTo(x1.Score));

                var x2 =
                    orderRated.RatingScores.Single(
                        x => x.RatingTypeId == ratingScoreDetailsDto.ElementAt(1).RatingTypeId);
                Assert.That(ratingScoreDetailsDto.ElementAt(1).Score, Is.EqualTo(x2.Score));
            }
        }

        [Test]
        public void when_payment_information_set()
        {
            var creditCardId = Guid.NewGuid();
            Sut.Handle(new PaymentInformationSet
            {
                SourceId = _orderId,
                CreditCardId = creditCardId,
                TipAmount = 5,
                TipPercent = 10
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.IsTrue(dto.PaymentInformation.PayWithCreditCard);
                Assert.AreEqual(creditCardId, dto.PaymentInformation.CreditCardId);
                Assert.AreEqual(5, dto.PaymentInformation.TipAmount);
                Assert.AreEqual(10, dto.PaymentInformation.TipPercent);
            }
        }

        [Test]
        public void when_faret_information_set()
        {
            double fare = 12;
            double tip = 2;
            double toll = 5;
            double tax = 3;
            Sut.Handle(new OrderFareUpdated
            {
                SourceId = _orderId,
                Fare = fare,
                Tip = tip,
                Toll = toll,
                Tax = tax,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.AreEqual(fare, dto.Fare);
                Assert.AreEqual(tip, dto.Tip);
                Assert.AreEqual(tax, dto.Tax);
                Assert.AreEqual(toll, dto.Toll);

                var details = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(details);
                Assert.IsTrue(details.FareAvailable);
            }
        }

        [Test]
        public void when_removed_then_dto_updated()
        {
            var orderRemovedFromHistory = new OrderRemovedFromHistory {SourceId = _orderId};
            Sut.Handle(orderRemovedFromHistory);

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.IsTrue(dto.IsRemovedFromHistory);
            }
        }
    }
}