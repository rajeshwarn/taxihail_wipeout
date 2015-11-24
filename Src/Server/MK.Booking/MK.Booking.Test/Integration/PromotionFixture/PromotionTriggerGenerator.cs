using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Booking.SMS;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.PromotionFixture
{
    public class given_a_promotion_view_model_generator : given_a_read_model_database
    {
        protected readonly List<ICommand> Commands = new List<ICommand>();
        protected readonly PromotionDetailGenerator PromoGenerator;
        protected readonly CreditCardPaymentDetailsGenerator CreditCardGenerator;
        protected readonly OrderGenerator OrderGenerator;
        protected readonly PromotionTriggerGenerator TriggerSut;


        public given_a_promotion_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            var orderDetailProjectionSet = new EntityProjectionSet<OrderDetail>(() => new BookingDbContext(DbName));
            var orderStatusDetailProjectionSet = new EntityProjectionSet<OrderStatusDetail>(() => new BookingDbContext(DbName));

            PromoGenerator = new PromotionDetailGenerator(() => new BookingDbContext(DbName), new EntityProjectionSet<AccountDetail>(() => new BookingDbContext(DbName)));
            OrderGenerator = new OrderGenerator(() => new BookingDbContext(DbName), orderDetailProjectionSet, orderStatusDetailProjectionSet, new OrderRatingEntityProjectionSet(() => new BookingDbContext(DbName)), new Logger(), new TestServerSettings());
            CreditCardGenerator = new CreditCardPaymentDetailsGenerator(() => new BookingDbContext(DbName), orderDetailProjectionSet, orderStatusDetailProjectionSet, new TestServerSettings());

            TriggerSut = new PromotionTriggerGenerator(() => new BookingDbContext(DbName), bus.Object,
                new PromotionDao(() => new BookingDbContext(DbName), new SystemClock(), new TestServerSettings(), null), new AccountDao(() => new BookingDbContext(DbName)), new OrderDao(() => new BookingDbContext(DbName)));
        }
    }

    [TestFixture]
    public class given_a_new_account_trigger_promotion : given_a_promotion_view_model_generator
    {
        private readonly Guid _promoAccountTriggerId = Guid.NewGuid();
        private readonly Guid _promoAmountSpentTriggerId = Guid.NewGuid();
        private readonly Guid _promoRideCountTriggerId = Guid.NewGuid();

        private readonly OrderCreated _orderCreatedCommand;

        public given_a_new_account_trigger_promotion()
        {
            _orderCreatedCommand = new OrderCreated
            {
                PickupAddress = new Address
                {
                    Apartment = "3939",
                    Street = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                },
                PickupDate = new DateTime(2014, 11, 10),
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
                CreatedDate = new DateTime(2014, 11, 10),
                ClientLanguageCode = "fr",
                UserAgent = "TestUserAgent",
                ClientVersion = "1.0.0",
                UserNote = "une note"
            };

            var promo = new PromotionCreated
            {
                Name = "promo2",
                Description = "promodesc2",
                Code = "code",
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = false,
                DiscountType = PromoDiscountType.Percentage,
                DiscountValue = 10,
                DaysOfWeek =
                    new[]
                    {
                        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
                        DayOfWeek.Saturday, DayOfWeek.Sunday
                    },
                MaxUsage = 2,
                MaxUsagePerUser = 1,
                StartDate = new DateTime(2013, 11, 10),
                EndDate = new DateTime(2020, 11, 10),
                StartTime =
                    new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month,
                        SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                EndTime =
                    new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month,
                        SqlDateTime.MinValue.Value.Day, 14, 0, 0),
                PublishedStartDate = new DateTime(2013, 11, 10),
                PublishedEndDate = new DateTime(2020, 11, 10),
            };

            promo.SourceId = _promoAccountTriggerId;
            promo.TriggerSettings = new PromotionTriggerSettings {Type = PromotionTriggerTypes.AccountCreated};
            PromoGenerator.Handle(promo);

            promo.SourceId = _promoAmountSpentTriggerId;
            promo.TriggerSettings = new PromotionTriggerSettings {Type = PromotionTriggerTypes.AmountSpent, AmountSpent = 20 };
            PromoGenerator.Handle(promo);

            promo.SourceId = _promoRideCountTriggerId;
            promo.TriggerSettings = new PromotionTriggerSettings {Type = PromotionTriggerTypes.RideCount, RideCount = 2 };
            PromoGenerator.Handle(promo);


            PromoGenerator.Handle(new PromotionActivated { SourceId = _promoAccountTriggerId });
            PromoGenerator.Handle(new PromotionActivated { SourceId = _promoAmountSpentTriggerId });
            PromoGenerator.Handle(new PromotionActivated { SourceId = _promoRideCountTriggerId });
        }

        [Test]
        public void when_new_account_created_then_promotion_unlocked()
        {
            var accountId = Guid.NewGuid();

            TriggerSut.Handle(new AccountRegistered { SourceId = accountId, Email = "test@test.com", Language = "en"});

            Thread.Sleep(500);

            var commands = Commands.OfType<AddUserToPromotionWhiteList>().Where(c => c.AccountIds.Contains(accountId)).ToArray();

            Assert.AreEqual(1, commands.Count());
            Assert.AreEqual(null, commands[0].LastTriggeredAmount);
        }

        [Test]
        public void when_ride_count_attained_then_promotion_unlocked()
        {
            var accountId = Guid.NewGuid();
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();

            var newOrderStatus = new OrderStatusChanged
            {
                IsCompleted = true,
                Status = new OrderStatusDetail { AccountId = accountId, Status = OrderStatus.Completed }
            };

            _orderCreatedCommand.AccountId = accountId;
            _orderCreatedCommand.SourceId = orderId1;
            newOrderStatus.SourceId = orderId1;

            OrderGenerator.Handle(_orderCreatedCommand);
            OrderGenerator.Handle(newOrderStatus);
            TriggerSut.Handle(newOrderStatus);

            _orderCreatedCommand.SourceId = orderId2;
            newOrderStatus.SourceId = orderId2;

            OrderGenerator.Handle(_orderCreatedCommand);
            OrderGenerator.Handle(newOrderStatus);
            TriggerSut.Handle(newOrderStatus);

            var commands = Commands.OfType<AddUserToPromotionWhiteList>().Where(c => c.AccountIds.Contains(accountId)).ToArray();

            Assert.AreEqual(1, commands.Count());
            Assert.AreEqual(2, commands[0].LastTriggeredAmount);
        }

        [Test]
        public void when_amount_spent_attained_then_promotion_unlocked()
        {
            var accountId = Guid.NewGuid();
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();

            var newOrderStatus = new OrderStatusChanged
            {
                IsCompleted = true,
                Status = new OrderStatusDetail { AccountId = accountId, Status = OrderStatus.Completed }
            };

            var newPaymentInit = new CreditCardPaymentInitiated
            {
                Meter = (decimal)15.31
            };

            var newPayment = new CreditCardPaymentCaptured_V2
            {
                FeeType = FeeTypes.None,
                Meter = (decimal)15.31,
                AuthorizationCode = Guid.NewGuid().ToString(),
                AccountId = accountId
            };

            _orderCreatedCommand.AccountId = accountId;

            _orderCreatedCommand.SourceId = orderId1;
            newOrderStatus.SourceId = orderId1;
            newPaymentInit.SourceId = orderId1;
            newPaymentInit.OrderId = orderId1;
            newPayment.SourceId = orderId1;
            newPayment.OrderId = orderId1;

            OrderGenerator.Handle(_orderCreatedCommand);
            OrderGenerator.Handle(newOrderStatus);
            CreditCardGenerator.Handle(newPaymentInit);
            CreditCardGenerator.Handle(newPayment);
            TriggerSut.Handle(newPayment);

            _orderCreatedCommand.SourceId = orderId2;
            newOrderStatus.SourceId = orderId2;
            newPaymentInit.SourceId = orderId2;
            newPaymentInit.OrderId = orderId2;
            newPayment.SourceId = orderId2;
            newPayment.OrderId = orderId2;

            OrderGenerator.Handle(_orderCreatedCommand);
            OrderGenerator.Handle(newOrderStatus);
            CreditCardGenerator.Handle(newPaymentInit);
            CreditCardGenerator.Handle(newPayment);
            TriggerSut.Handle(newPayment);

            var commands = Commands.OfType<AddUserToPromotionWhiteList>().Where(c => c.AccountIds.Contains(accountId)).ToArray();

            Assert.AreEqual(1, commands.Count());
            Assert.AreEqual(30.62, commands[0].LastTriggeredAmount);
        }
    }
}
