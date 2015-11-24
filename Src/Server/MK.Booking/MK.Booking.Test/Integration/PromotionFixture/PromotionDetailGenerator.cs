using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.PromotionFixture
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected PromotionDetailGenerator Sut;

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new PromotionDetailGenerator(() => new BookingDbContext(DbName), new EntityProjectionSet<AccountDetail>(() => new BookingDbContext(DbName)));
        }
    }

    [TestFixture]
    public class given_no_promotion : given_a_view_model_generator
    {
        [Test]
        public void when_promotion_created_then_promotion_dto_populated()
        {
            var promoId = Guid.NewGuid();

            Sut.Handle(new PromotionCreated
            {
                SourceId = promoId,
                Name = "promo1",
                Description = "promodesc1",
                Code = "code",
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = false,
                DiscountType = PromoDiscountType.Percentage,
                DiscountValue = 10,
                DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                MaxUsage = 2,
                MaxUsagePerUser = 1,
                StartDate = new DateTime(2014, 11, 10),
                EndDate = new DateTime(2015, 11, 10),
                StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0),
                PublishedStartDate = new DateTime(2014, 11, 10),
                PublishedEndDate = new DateTime(2015, 11, 10),
                TriggerSettings = new PromotionTriggerSettings()
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<PromotionDetail>(promoId);

                Assert.NotNull(dto);
                Assert.AreEqual(promoId, dto.Id);
                Assert.AreEqual(true, dto.Active);
                Assert.AreEqual("promo1", dto.Name);
                Assert.AreEqual("promodesc1", dto.Description);
                Assert.AreEqual("code", dto.Code);
                Assert.AreEqual(true, dto.AppliesToCurrentBooking);
                Assert.AreEqual(false, dto.AppliesToFutureBooking);
                Assert.AreEqual(PromoDiscountType.Percentage, dto.DiscountType);
                Assert.AreEqual(10, dto.DiscountValue);
                Assert.AreEqual(2, dto.MaxUsage);
                Assert.AreEqual(1, dto.MaxUsagePerUser);
                Assert.AreEqual("[\"Monday\",\"Tuesday\"]", dto.DaysOfWeek);
                Assert.AreEqual(new DateTime(2014, 11, 10), dto.StartDate);
                Assert.AreEqual(new DateTime(2015, 11, 10), dto.EndDate);
                Assert.AreEqual(SqlDateTime.MinValue.Value.Date, dto.StartTime.Value.Date);
                Assert.AreEqual(SqlDateTime.MinValue.Value.Date, dto.EndTime.Value.Date);
                Assert.AreEqual(new TimeSpan(10, 0, 0), dto.StartTime.Value.TimeOfDay);
                Assert.AreEqual(new TimeSpan(14, 0, 0), dto.EndTime.Value.TimeOfDay);
                Assert.AreEqual(new DateTime(2014, 11, 10), dto.PublishedStartDate);
                Assert.AreEqual(new DateTime(2015, 11, 10), dto.PublishedEndDate);
                Assert.AreEqual(PromotionTriggerTypes.NoTrigger, dto.TriggerSettings.Type);
                Assert.AreEqual(0, dto.TriggerSettings.RideCount);
                Assert.AreEqual(0, dto.TriggerSettings.AmountSpent);
            }
        }
    }

    [TestFixture]
    public class given_existing_promotion : given_a_view_model_generator
    {
        private Guid _promoId = Guid.NewGuid();

        public given_existing_promotion()
        {
            Sut.Handle(new PromotionCreated
            {
                SourceId = _promoId,
                Name = "promo1",
                Description = "promodesc1",
                Code = "code",
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = false,
                DiscountType = PromoDiscountType.Percentage,
                DiscountValue = 10,
                DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                MaxUsage = 2,
                MaxUsagePerUser = 1,
                StartDate = new DateTime(2014, 11, 10),
                EndDate = new DateTime(2015, 11, 10),
                StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0),
                PublishedStartDate = new DateTime(2014, 11, 10),
                PublishedEndDate = new DateTime(2015, 11, 10),
                TriggerSettings = new PromotionTriggerSettings()
            });
        }

        [Test]
        public void when_promotion_updated_then_dto_updated()
        {
            Sut.Handle(new PromotionUpdated
            {
                SourceId = _promoId,
                Name = "promo2",
                Description = "promodesc2",
                Code = "code2",
                AppliesToCurrentBooking = false,
                AppliesToFutureBooking = true,
                DiscountType = PromoDiscountType.Cash,
                DiscountValue = 15,
                DaysOfWeek = new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                MaxUsage = 5,
                TriggerSettings = new PromotionTriggerSettings { Type = PromotionTriggerTypes.RideCount, RideCount = 10 }
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<PromotionDetail>(_promoId);

                Assert.NotNull(dto);
                Assert.AreEqual(_promoId, dto.Id);
                Assert.AreEqual("promo2", dto.Name);
                Assert.AreEqual("code2", dto.Code);
                Assert.AreEqual(false, dto.AppliesToCurrentBooking);
                Assert.AreEqual(true, dto.AppliesToFutureBooking);
                Assert.AreEqual(PromoDiscountType.Cash, dto.DiscountType);
                Assert.AreEqual(15, dto.DiscountValue);
                Assert.AreEqual(5, dto.MaxUsage);
                Assert.AreEqual(null, dto.MaxUsagePerUser);
                Assert.AreEqual("[\"Wednesday\",\"Thursday\",\"Friday\"]", dto.DaysOfWeek);
                Assert.AreEqual(null, dto.StartDate);
                Assert.AreEqual(null, dto.EndDate);
                Assert.AreEqual(null, dto.StartTime);
                Assert.AreEqual(null, dto.EndTime);
                Assert.AreEqual(null, dto.PublishedStartDate);
                Assert.AreEqual(null, dto.PublishedEndDate);
                
                Assert.AreEqual(PromotionTriggerTypes.RideCount, dto.TriggerSettings.Type);
                Assert.AreEqual(10, dto.TriggerSettings.RideCount);
            }
        }

        [Test]
        public void when_promotion_deactivated_then_dto_updated()
        {
            Sut.Handle(new PromotionDeactivated { SourceId = _promoId });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<PromotionDetail>(_promoId);

                Assert.NotNull(dto);
                Assert.AreEqual(_promoId, dto.Id);
                Assert.AreEqual(false, dto.Active);
            }
        }

        [Test]
        public void when_promotion_activated_then_dto_updated()
        {
            Sut.Handle(new PromotionActivated { SourceId = _promoId });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<PromotionDetail>(_promoId);

                Assert.NotNull(dto);
                Assert.AreEqual(_promoId, dto.Id);
                Assert.AreEqual(true, dto.Active);
            }
        }

        [Test]
        public void when_promotion_applied_then_dto_created()
        {
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    Email = "test@test.com",
                    CreationDate = DateTime.Now
                });
            }

            Sut.Handle(new PromotionApplied
            {
                SourceId = _promoId,
                AccountId = accountId,
                OrderId = orderId,
                Code = "code",
                DiscountType = PromoDiscountType.Cash,
                DiscountValue = 10
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<PromotionUsageDetail>(orderId);

                Assert.NotNull(dto);
                Assert.AreEqual(_promoId, dto.PromoId);
                Assert.AreEqual(orderId, dto.OrderId);
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual("code", dto.Code);
                Assert.AreEqual(PromoDiscountType.Cash, dto.DiscountType);
                Assert.AreEqual(10, dto.DiscountValue);
            }
        }

        [Test]
        public void when_promotion_redeemed_then_dto_updated()
        {
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    Email = "test@test.com",
                    CreationDate = DateTime.Now
                });
            }

            Sut.Handle(new PromotionApplied
            {
                SourceId = _promoId,
                AccountId = accountId,
                Code = "code",
                DiscountType = PromoDiscountType.Cash,
                DiscountValue = 10,
                OrderId = orderId
            });

            Sut.Handle(new PromotionRedeemed
            {
                SourceId = _promoId,
                OrderId = orderId,
                AmountSaved = 10
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<PromotionUsageDetail>(orderId);

                Assert.NotNull(dto);
                Assert.AreEqual(_promoId, dto.PromoId);
                Assert.AreEqual(orderId, dto.OrderId);
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual("code", dto.Code);
                Assert.AreEqual(PromoDiscountType.Cash, dto.DiscountType);
                Assert.AreEqual(10, dto.DiscountValue);
                Assert.AreEqual(10, dto.AmountSaved);
            }
        }
    }
}