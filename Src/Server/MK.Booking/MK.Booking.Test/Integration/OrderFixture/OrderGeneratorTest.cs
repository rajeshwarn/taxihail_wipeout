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

            Sut = new OrderGenerator(() => new BookingDbContext(DbName), new Logger(), new TestServerSettings());
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

    [TestFixture]
    public class given_no_manual_ridelinq_order : given_a_view_model_generator
    {
        [Test]
        public void when_order_paired_then_order_dto_populated()
        {
            var @event = new OrderManuallyPairedForRideLinq
            {
                SourceId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                TripId = 15,
                StartTime = DateTime.Now.AddMinutes(-5),
                PairingDate = DateTime.Now,
                PickupAddress = new Address
                {
                    Apartment = "3939",
                    Street = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                },
                UserAgent = "useragent",
                ClientLanguageCode = "en",
                ClientVersion = "1.0",
                OriginatingIpAddress = "192.168.12.30",
                KountSessionId = "1i3u13n123",

                Medallion = "1251515",
                DriverId = 124135356,

                PairingCode = "515152",
                PairingToken = "62523",
            };
            Sut.Handle(@event);

            using (var context = new BookingDbContext(DbName))
            {
                var order = context.Query<OrderDetail>().SingleOrDefault(x => x.Id == @event.SourceId);
                Assert.IsNotNull(order);
                Assert.AreEqual(@event.AccountId, order.AccountId);
                Assert.AreEqual(@event.TripId, order.IBSOrderId);
                Assert.AreEqual(@event.StartTime.Value.ToLongDateString(), order.PickupDate.ToLongDateString());
                Assert.AreEqual(@event.PairingDate.ToLongDateString(), order.CreatedDate.ToLongDateString());
                Assert.AreEqual(@event.PickupAddress.DisplayLine1, order.PickupAddress.DisplayLine1);
                Assert.AreEqual((int)OrderStatus.Created, order.Status);
                Assert.AreEqual(@event.UserAgent, order.UserAgent);
                Assert.AreEqual(@event.ClientLanguageCode, order.ClientLanguageCode);
                Assert.AreEqual(@event.ClientVersion, order.ClientVersion);
                Assert.AreEqual(true, order.IsManualRideLinq);
                Assert.AreEqual(@event.OriginatingIpAddress, order.OriginatingIpAddress);
                Assert.AreEqual(@event.KountSessionId, order.KountSessionId);

                var orderStatus = context.Query<OrderStatusDetail>().SingleOrDefault(x => x.OrderId == @event.SourceId);
                Assert.IsNotNull(orderStatus);
                Assert.AreEqual(@event.AccountId, orderStatus.AccountId);
                Assert.AreEqual(OrderStatus.Created, orderStatus.Status);
                Assert.AreEqual("Processing your order...", orderStatus.IBSStatusDescription);
                Assert.AreEqual(@event.StartTime.Value.ToLongDateString(), orderStatus.PickupDate.ToLongDateString());
                Assert.AreEqual(@event.Medallion, orderStatus.VehicleNumber);
                Assert.AreEqual(@event.DriverId.ToString(), orderStatus.DriverInfos.DriverId);

                var orderRideLinq = context.Query<OrderManualRideLinqDetail>().SingleOrDefault(x => x.OrderId == @event.SourceId);
                Assert.IsNotNull(orderRideLinq);
                Assert.AreEqual(@event.AccountId, orderRideLinq.AccountId);
                Assert.AreEqual(@event.PairingCode, orderRideLinq.PairingCode);
                Assert.AreEqual(@event.PairingToken, orderRideLinq.PairingToken);
                Assert.AreEqual(@event.PairingDate.ToLongDateString(), orderRideLinq.PairingDate.ToLongDateString());
                Assert.AreEqual(@event.StartTime.Value.ToLongDateString(), orderRideLinq.StartTime.Value.ToLongDateString());
                Assert.AreEqual(@event.Distance, orderRideLinq.Distance);
                Assert.AreEqual(@event.Extra, orderRideLinq.Extra);
                Assert.AreEqual(@event.Fare, orderRideLinq.Fare);
                Assert.AreEqual(@event.FareAtAlternateRate, orderRideLinq.FareAtAlternateRate);
                Assert.AreEqual(@event.Total, orderRideLinq.Total);
                Assert.AreEqual(@event.Toll, orderRideLinq.Toll);
                Assert.AreEqual(@event.Tax, orderRideLinq.Tax);
                Assert.AreEqual(@event.Tip, orderRideLinq.Tip);
                Assert.AreEqual(@event.Surcharge, orderRideLinq.Surcharge);
                Assert.AreEqual(@event.RateAtTripStart, orderRideLinq.RateAtTripStart);
                Assert.AreEqual(@event.RateAtTripEnd, orderRideLinq.RateAtTripEnd);
                Assert.AreEqual(@event.RateChangeTime, orderRideLinq.RateChangeTime);
                Assert.AreEqual(@event.Medallion, orderRideLinq.Medallion);
                Assert.AreEqual(@event.DeviceName, orderRideLinq.DeviceName);
                Assert.AreEqual(@event.TripId, orderRideLinq.TripId);
                Assert.AreEqual(@event.DriverId, orderRideLinq.DriverId);
                Assert.AreEqual(@event.LastFour, orderRideLinq.LastFour);
                Assert.AreEqual(@event.AccessFee, orderRideLinq.AccessFee);
            }
        }
    }

    [TestFixture]
    public class given_existing_manual_ridelinq_order : given_a_view_model_generator
    {
        private readonly Guid _orderId = Guid.NewGuid();
        private readonly DateTime _eventDate = DateTime.Now;

        public given_existing_manual_ridelinq_order()
        {
            var @event = new OrderManuallyPairedForRideLinq
            {
                EventDate = _eventDate,

                SourceId = _orderId,
                AccountId = Guid.NewGuid(),
                TripId = 15,
                StartTime = DateTime.Now.AddMinutes(-5),
                PairingDate = DateTime.Now,
                PickupAddress = new Address
                {
                    Apartment = "3939",
                    Street = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                },
                UserAgent = "useragent",
                ClientLanguageCode = "en",
                ClientVersion = "1.0",
                OriginatingIpAddress = "192.168.12.30",
                KountSessionId = "1i3u13n123",

                Medallion = "1251515",
                DriverId = 124135356,

                PairingCode = "515152",
                PairingToken = "62523",
            };
            Sut.Handle(@event);
        }

        [Test]
        public void when_unpaired_from_vehicle()
        {
            Sut.Handle(new OrderUnpairedFromManualRideLinq
            {
                SourceId = _orderId,
                EventDate = _eventDate
            });

            using (var context = new BookingDbContext(DbName))
            {
                var order = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(order);
                Assert.AreEqual((int)OrderStatus.Canceled, order.Status);

                var orderStatus = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(orderStatus);
                Assert.AreEqual(OrderStatus.Canceled, orderStatus.Status);

                var rideLinq = context.Find<OrderManualRideLinqDetail>(_orderId);
                Assert.NotNull(rideLinq);
                Assert.AreEqual(true, rideLinq.IsCancelled);
                Assert.AreEqual(_eventDate.ToLongDateString(), rideLinq.EndTime.Value.ToLongDateString());
            }
        }

        [Test]
        public void when_trip_updated_with_endtime()
        {
            Sut.Handle(new ManualRideLinqTripInfoUpdated
            {
                SourceId = _orderId,
                EndTime = DateTime.Now
            });

            using (var context = new BookingDbContext(DbName))
            {
                var order = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(order);
                Assert.AreEqual((int)OrderStatus.Completed, order.Status);
                Assert.AreEqual(true, order.DropOffDate.HasValue);

                var orderStatus = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(orderStatus);
                Assert.AreEqual(OrderStatus.Completed, orderStatus.Status);

                var rideLinq = context.Find<OrderManualRideLinqDetail>(_orderId);
                Assert.NotNull(rideLinq);
                Assert.AreEqual(true, rideLinq.EndTime.HasValue);
            }
        }

        [Test]
        public void when_trip_updated_with_pairing_error()
        {
            Sut.Handle(new ManualRideLinqTripInfoUpdated
            {
                SourceId = _orderId,
                PairingError = "error"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var order = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(order);
                Assert.AreEqual((int)OrderStatus.Canceled, order.Status);

                var orderStatus = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(orderStatus);
                Assert.AreEqual(OrderStatus.Canceled, orderStatus.Status);

                var rideLinq = context.Find<OrderManualRideLinqDetail>(_orderId);
                Assert.NotNull(rideLinq);
                Assert.AreEqual("error", rideLinq.PairingError);
            }
        }

        [Test]
        public void when_trip_status_changed_to_waitingforpayment()
        {
            var now = DateTime.Now;
            Sut.Handle(new OrderStatusChangedForManualRideLinq
            {
                SourceId = _orderId,
                Status = OrderStatus.WaitingForPayment,
                LastTripPollingDateInUtc = now
            });

            using (var context = new BookingDbContext(DbName))
            {
                var order = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(order);
                Assert.AreEqual((int)OrderStatus.WaitingForPayment, order.Status);

                var orderStatus = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(orderStatus);
                Assert.AreEqual(OrderStatus.WaitingForPayment, orderStatus.Status);
                Assert.AreEqual(now.ToLongDateString(), orderStatus.LastTripPollingDateInUtc.Value.ToLongDateString());

                var rideLinq = context.Find<OrderManualRideLinqDetail>(_orderId);
                Assert.NotNull(rideLinq);
                Assert.AreEqual(false, rideLinq.EndTime.HasValue);
                Assert.AreEqual(true, rideLinq.IsWaitingForPayment);
            }
        }

        [Test]
        public void when_trip_status_changed_to_timedout()
        {
            var now = DateTime.Now;
            Sut.Handle(new OrderStatusChangedForManualRideLinq
            {
                SourceId = _orderId,
                Status = OrderStatus.TimedOut,
                LastTripPollingDateInUtc = now
            });

            using (var context = new BookingDbContext(DbName))
            {
                var order = context.Find<OrderDetail>(_orderId);
                Assert.NotNull(order);
                Assert.AreEqual((int)OrderStatus.TimedOut, order.Status);

                var orderStatus = context.Find<OrderStatusDetail>(_orderId);
                Assert.NotNull(orderStatus);
                Assert.AreEqual(OrderStatus.TimedOut, orderStatus.Status);
                Assert.AreEqual(now.ToLongDateString(), orderStatus.LastTripPollingDateInUtc.Value.ToLongDateString());

                var rideLinq = context.Find<OrderManualRideLinqDetail>(_orderId);
                Assert.NotNull(rideLinq);
                Assert.AreEqual(true, rideLinq.EndTime.HasValue);
                Assert.AreEqual(false, rideLinq.IsWaitingForPayment);
            }
        }
    }
}