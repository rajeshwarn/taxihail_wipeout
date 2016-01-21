#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

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

            Sut = new OrderGenerator(
                new EntityProjectionSet<OrderDetail>(() => new BookingDbContext(DbName)),
                new EntityProjectionSet<OrderStatusDetail>(() => new BookingDbContext(DbName)),
                new OrderRatingEntityProjectionSet(() => new BookingDbContext(DbName)),
                new EntityProjectionSet<OrderPairingDetail>(() => new BookingDbContext(DbName)),
                new EntityProjectionSet<OrderManualRideLinqDetail>(() => new BookingDbContext(DbName)),
                new EntityProjectionSet<OrderNotificationDetail>(() => new BookingDbContext(DbName)),
                new Logger(), 
                new TestServerSettings());
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
                    LargeBags = 92,
                    AccountNumber = "account",
                    CustomerNumber = "customer",
                    PayBack = "123"
                },
                CreatedDate = createdDate,
                ClientLanguageCode = "fr",
                UserAgent = "TestUserAgent",
                ClientVersion = "1.0.0",
                UserNote = "une note",
                BookingFees = 5m,
                Market = "MTL",
                CompanyKey = "Kramerica",
                CompanyName = "Kramerica Industries",
                EstimatedFare = 50.5,
                IsChargeAccountPaymentWithCardOnFile = true,
                IsPrepaid = true,
                OriginatingIpAddress = "192.168.12.30",
                KountSessionId = "1i3u13n123"
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
                Assert.AreEqual("une note", dto.UserNote);
                Assert.AreEqual(5, dto.BookingFees);
                Assert.AreEqual("MTL", dto.Market);
                Assert.AreEqual("Kramerica", dto.CompanyKey);
                Assert.AreEqual("Kramerica Industries", dto.CompanyName);
                Assert.AreEqual(50.5, dto.EstimatedFare);
                Assert.AreEqual("192.168.12.30", dto.OriginatingIpAddress);
                Assert.AreEqual("1i3u13n123", dto.KountSessionId);

                //Settings
                Assert.AreEqual(99, dto.Settings.ChargeTypeId);
                Assert.AreEqual(98, dto.Settings.VehicleTypeId);
                Assert.AreEqual(97, dto.Settings.ProviderId);
                Assert.AreEqual(96, dto.Settings.NumberOfTaxi);
                Assert.AreEqual(95, dto.Settings.Passengers);
                Assert.AreEqual("94", dto.Settings.Phone);
                Assert.AreEqual("93", dto.Settings.Name);
                Assert.AreEqual(92, dto.Settings.LargeBags);
                Assert.AreEqual("account", dto.Settings.AccountNumber);
                Assert.AreEqual("customer", dto.Settings.CustomerNumber);
                Assert.AreEqual("123", dto.Settings.PayBack);
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
                Fare = 23,
                Toll = 2,
                Tip = 5,
                Tax = 12,
                Surcharge = 1,
                IsCompleted = true
            };
            Sut.Handle(orderCompleted);

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.NotNull(dto.Status == (int) OrderStatus.Completed);
                Assert.AreEqual(orderCompleted.Fare, dto.Fare);
                Assert.AreEqual(orderCompleted.Toll, dto.Toll);
                Assert.AreEqual(orderCompleted.Tip, dto.Tip);
                Assert.AreEqual(orderCompleted.Tax, dto.Tax);
                Assert.AreEqual(orderCompleted.Surcharge, dto.Surcharge);
            }
        }


        [Test]
        public void when_order_rated_then_order_dto_populated()
        {
            var orderRated = new OrderRated
            {
				AccountId = _accountId,
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
        public void when_fare_information_set()
        {
            double fare = 12;
            double tip = 2;
            double toll = 5;
            double tax = 3;
            Sut.Handle(new OrderStatusChanged
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

        [Test]
        public void when_order_dispatch_company_changed_then_dto_updated()
        {
            const string dispatchCompanyName = "Kukai Foundation";
            const string dispatchCompanyKey = "123456";

            Sut.Handle(new OrderPreparedForNextDispatch
            {
                SourceId = _orderId,
                DispatchCompanyName = dispatchCompanyName,
                DispatchCompanyKey = dispatchCompanyKey
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.AreEqual(_orderId, dto.OrderId);
                Assert.AreEqual(dispatchCompanyName, dto.NextDispatchCompanyName);
                Assert.AreEqual(dispatchCompanyKey, dto.NextDispatchCompanyKey);
                Assert.AreEqual(OrderStatus.TimedOut, dto.Status);
            }
        }

        [Test]
        public void when_order_switched_to_next_dispatch_company_then_dto_updated()
        {
            const int ibsOrderId = 98754;
            const string dispatchCompanyKey = "x2s42";
            const string dispatchCompanyName = "Vector Industries";

            Sut.Handle(new OrderSwitchedToNextDispatchCompany
            {
                SourceId = _orderId,
                IBSOrderId = ibsOrderId,
                CompanyKey = dispatchCompanyKey,
                CompanyName = dispatchCompanyName
            });

            using (var context = new BookingDbContext(DbName))
            {
                var orderDto = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(orderDto);
                Assert.AreEqual(_orderId, orderDto.Id);
                Assert.AreEqual(ibsOrderId, orderDto.IBSOrderId);
                Assert.AreEqual(dispatchCompanyKey, orderDto.CompanyKey);

                var statusDto = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(statusDto);
                Assert.AreEqual(_orderId, statusDto.OrderId);
                Assert.AreEqual(ibsOrderId, statusDto.IBSOrderId);
                Assert.AreEqual(dispatchCompanyKey, statusDto.CompanyKey);
                Assert.IsNull(statusDto.NextDispatchCompanyKey);
                Assert.IsNull(statusDto.NextDispatchCompanyName);
                Assert.IsNull(statusDto.NetworkPairingTimeout);
                Assert.AreEqual(OrderStatus.Created, statusDto.Status);
            }
        }

        [Test]
        public void when_dispatch_company_switch_ignored_then_dto_updated()
        {
            Sut.Handle(new DispatchCompanySwitchIgnored
            {
                SourceId = _orderId
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(dto);
                Assert.AreEqual(true, dto.IgnoreDispatchCompanySwitch);
                Assert.AreEqual(OrderStatus.Created, dto.Status);
                Assert.IsNull(dto.NextDispatchCompanyKey);
                Assert.IsNull(dto.NextDispatchCompanyName);
            }
        }
    }
}