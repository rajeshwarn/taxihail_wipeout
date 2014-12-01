using System;
using System.Data.SqlTypes;
using System.Linq;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.PromotionFixture
{
    //TODO PROMO how do we check that an order already get a promo code i.e. no multiple promo codes for an order?
    [TestFixture]
    public class given_one_promotion
    {
        private Guid _promoId = Guid.NewGuid();
        private string _code = "code";
        private PromoDiscountType _type = PromoDiscountType.Percentage;
        private decimal _value = 10;

        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Promotion>();

            _sut.Setup(new PromotionCommandHandler(_sut.Repository));
            _sut.Given(new PromotionCreated
            {
                SourceId = _promoId,
                Name = "promo1",
                Description = "promodesc1",
                Code = _code,
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = false,
                DiscountType = _type,
                DiscountValue = _value,
                DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                MaxUsage = 2,
                MaxUsagePerUser = 1,
                StartDate = new DateTime(2014, 11, 10),
                EndDate = new DateTime(2015, 11, 10),
                StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0),
                PublishedStartDate = new DateTime(2014, 11, 9),
                PublishedEndDate = new DateTime(2015, 11, 10)
            });
        }

        private EventSourcingTestHelper<Promotion> _sut;
        
        [Test]
        public void when_updating_promo_successfully()
        {
            _sut.When(new UpdatePromotion
            {
                PromoId = _promoId, 
                Name = "promo2",
                Description = "promodesc2",
                Code = "code2",
                AppliesToCurrentBooking = false,
                AppliesToFutureBooking = true,
                DiscountType = PromoDiscountType.Cash,
                DiscountValue = 15,
                DaysOfWeek = new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                MaxUsage = 5
            });

            var @event = _sut.ThenHasSingle<PromotionUpdated>();
            Assert.AreEqual(_promoId, @event.SourceId);
            Assert.AreEqual("promo2", @event.Name);
            Assert.AreEqual("promodesc2", @event.Description);
            Assert.AreEqual("code2", @event.Code);
            Assert.AreEqual(false, @event.AppliesToCurrentBooking);
            Assert.AreEqual(true, @event.AppliesToFutureBooking);
            Assert.AreEqual(PromoDiscountType.Cash, @event.DiscountType);
            Assert.AreEqual(15, @event.DiscountValue);
            Assert.AreEqual(5, @event.MaxUsage);
            Assert.AreEqual(null, @event.MaxUsagePerUser);
            Assert.AreEqual(3, @event.DaysOfWeek.Count());
            Assert.AreEqual(null, @event.StartDate);
            Assert.AreEqual(null, @event.EndDate);
            Assert.AreEqual(null, @event.StartTime);
            Assert.AreEqual(null, @event.EndTime);
            Assert.AreEqual(null, @event.PublishedStartDate);
            Assert.AreEqual(null, @event.PublishedEndDate);
        }

        [Test]
        public void when_updating_a_promo_with_missing_required_fields()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new UpdatePromotion { PromoId = _promoId }));
            Assert.AreEqual("Missing required fields", ex.Message);
        }

        [Test]
        public void when_activating_promo_successfully()
        {
            _sut.When(new ActivatePromotion { PromoId = _promoId });

            var @event = _sut.ThenHasSingle<PromotionActivated>();
            Assert.AreEqual(_promoId, @event.SourceId);
        }

        [Test]
        public void when_deactivating_promo_successfully()
        {
            _sut.When(new DeactivatePromotion { PromoId = _promoId });

            var @event = _sut.ThenHasSingle<PromotionDeactivated>();
            Assert.AreEqual(_promoId, @event.SourceId);
        }

        [Test]
        public void when_applying_a_promo_successfully()
        {
            var orderId = Guid.NewGuid();
            var accountId = Guid.NewGuid();

            _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = accountId,
                OrderId = orderId,
                IsFutureBooking = false,
                PickupDate = new DateTime(2014, 11, 24, 13, 0, 0)
            });

            var @event = _sut.ThenHasSingle<PromotionApplied>();
            Assert.AreEqual(_promoId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
            Assert.AreEqual(accountId, @event.AccountId);
            Assert.AreEqual(_code, @event.Code);
            Assert.AreEqual(_type, @event.DiscountType);
            Assert.AreEqual(_value, @event.DiscountValue);
        }


        //TODO PROMO : should we get an error in this case?
        [Test]
        public void when_redeeming_a_promo_without_having_it_applied_first()
        {
            var orderId = Guid.NewGuid();

            _sut.Given(
                new PromotionUpdated
                {
                    SourceId = _promoId,
                    Name = "promo1",
                    Code = _code,
                    AppliesToCurrentBooking = false,
                    AppliesToFutureBooking = true,
                    DiscountType = PromoDiscountType.Cash,
                    DiscountValue = 15,
                    DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                    MaxUsage = 2,
                    MaxUsagePerUser = 1,
                    StartDate = new DateTime(2014, 11, 10),
                    EndDate = new DateTime(2015, 11, 10),
                    StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                    EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0)
                }
            );

            _sut.When(new RedeemPromotion
            {
                PromoId = _promoId,
                OrderId = orderId,
                TotalAmountOfOrder = 44.12m
            });

            var @event = _sut.ThenHasSingle<PromotionRedeemed>();
            Assert.AreEqual(_promoId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
            Assert.AreEqual(0, @event.AmountSaved);
        }

        [Test]
        public void when_redeeming_a_promo_successfully_with_order_amount_higher_than_cash_rebate()
        {
            var orderId = Guid.NewGuid();

            _sut.Given(
                new PromotionUpdated
                    {
                        SourceId = _promoId,
                        Name = "promo1",
                        Code = _code,
                        AppliesToCurrentBooking = false,
                        AppliesToFutureBooking = true,
                        DiscountType = PromoDiscountType.Cash,
                        DiscountValue = 15,
                        DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                        MaxUsage = 2,
                        MaxUsagePerUser = 1,
                        StartDate = new DateTime(2014, 11, 10),
                        EndDate = new DateTime(2015, 11, 10),
                        StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                        EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0)
                    },
                new PromotionApplied
                    {
                        SourceId = _promoId,
                        AccountId = Guid.NewGuid(),
                        Code = "promo1",
                        DiscountType = PromoDiscountType.Cash,
                        DiscountValue = 15,
                        OrderId = orderId
                    }
            );

            _sut.When(new RedeemPromotion
            {
                PromoId = _promoId,
                OrderId = orderId,
                TotalAmountOfOrder = 44.12m
            });

            var @event = _sut.ThenHasSingle<PromotionRedeemed>();
            Assert.AreEqual(_promoId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
            Assert.AreEqual(15, @event.AmountSaved);
        }

        [Test]
        public void when_redeeming_a_promo_successfully_with_order_amount_lower_than_cash_rebate()
        {
            var orderId = Guid.NewGuid();

            _sut.Given(
                new PromotionUpdated
                {
                    SourceId = _promoId,
                    Name = "promo1",
                    Code = _code,
                    AppliesToCurrentBooking = false,
                    AppliesToFutureBooking = true,
                    DiscountType = PromoDiscountType.Cash,
                    DiscountValue = 15,
                    DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                    MaxUsage = 2,
                    MaxUsagePerUser = 1,
                    StartDate = new DateTime(2014, 11, 10),
                    EndDate = new DateTime(2015, 11, 10),
                    StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                    EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0)
                },
                new PromotionApplied
                {
                    SourceId = _promoId,
                    AccountId = Guid.NewGuid(),
                    Code = "promo1",
                    DiscountType = PromoDiscountType.Cash,
                    DiscountValue = 15,
                    OrderId = orderId
                }
            );

            _sut.When(new RedeemPromotion
            {
                PromoId = _promoId,
                OrderId = orderId,
                TotalAmountOfOrder = 12.44m
            });

            var @event = _sut.ThenHasSingle<PromotionRedeemed>();
            Assert.AreEqual(_promoId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
            Assert.AreEqual(12.44, @event.AmountSaved);
        }

        [Test]
        public void when_redeeming_a_promo_successfully_with_percentage_rebate()
        {
            var orderId = Guid.NewGuid();

            _sut.Given(
                new PromotionUpdated
                {
                    SourceId = _promoId,
                    Name = "promo1",
                    Code = _code,
                    AppliesToCurrentBooking = false,
                    AppliesToFutureBooking = true,
                    DiscountType = PromoDiscountType.Percentage,
                    DiscountValue = 50,
                    DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                    MaxUsage = 2,
                    MaxUsagePerUser = 1,
                    StartDate = new DateTime(2014, 11, 10),
                    EndDate = new DateTime(2015, 11, 10),
                    StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                    EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0)
                },
                new PromotionApplied
                {
                    SourceId = _promoId,
                    AccountId = Guid.NewGuid(),
                    Code = "promo1",
                    DiscountType = PromoDiscountType.Percentage,
                    DiscountValue = 50,
                    OrderId = orderId
                }
            );

            _sut.When(new RedeemPromotion
            {
                PromoId = _promoId,
                OrderId = orderId,
                TotalAmountOfOrder = 44.12m
            });

            var expectedAmountSaved = 44.12 * 50/100;

            var @event = _sut.ThenHasSingle<PromotionRedeemed>();
            Assert.AreEqual(_promoId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
            Assert.AreEqual(expectedAmountSaved, @event.AmountSaved);
        }

        [Test]
        public void when_applying_a_promo_that_is_not_active()
        {
            _sut.Given(new PromotionDeactivated
            {
                SourceId = _promoId
            });

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                IsFutureBooking = false,
                PickupDate = new DateTime(2014, 11, 24, 13, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionIsNotActive", ex.Message);
        }

        [Test]
        public void when_applying_a_promo_that_have_passed_the_max_usage_by_system()
        {
            _sut.Given(
                new PromotionApplied { SourceId = _promoId, AccountId = Guid.NewGuid() },
                new PromotionApplied { SourceId = _promoId, AccountId = Guid.NewGuid() }
            );

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                IsFutureBooking = false,
                PickupDate = new DateTime(2014, 11, 24, 13, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionHasReachedMaxUsage", ex.Message);
        }

        [Test]
        public void when_applying_a_promo_that_have_passed_the_max_usage_by_user()
        {
            var accountId = Guid.NewGuid();

            _sut.Given(
                new PromotionApplied { SourceId = _promoId, AccountId = accountId }
            );

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = accountId,
                OrderId = Guid.NewGuid(),
                IsFutureBooking = false,
                PickupDate = new DateTime(2014, 11, 24, 13, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionHasReachedMaxUsage", ex.Message);
        }

        [Test]
        public void when_applying_a_promo_that_is_only_for_current_booking()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                IsFutureBooking = true,
                PickupDate = new DateTime(2014, 11, 24, 13, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionAppliesToCurrentBookingOnly", ex.Message);
        }

        [Test]
        public void when_applying_a_promo_that_is_only_for_future_booking()
        {
            _sut.Given(new PromotionUpdated
            {
                SourceId = _promoId,
                Name = "promo1",
                Code = _code,
                AppliesToCurrentBooking = false,
                AppliesToFutureBooking = true,
                DiscountType = _type,
                DiscountValue = _value,
                DaysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                MaxUsage = 2,
                MaxUsagePerUser = 1,
                StartDate = new DateTime(2014, 11, 10),
                EndDate = new DateTime(2015, 11, 10),
                StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0)
            });

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                IsFutureBooking = false,
                PickupDate = new DateTime(2014, 11, 24, 13, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionAppliesToFutureBookingOnly", ex.Message);
        }

        [Test]
        public void when_applying_a_promo_on_a_day_that_it_is_not_active()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                IsFutureBooking = false,
                PickupDate = new DateTime(2014, 11, 26, 13, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionNotAvailableForThisDayOfTheWeek", ex.Message);
        }

        [Test]
        public void when_applying_a_promo_that_did_not_start_yet()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                IsFutureBooking = false,
                PickupDate = new DateTime(2012, 9, 24, 13, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionNotStartedYet", ex.Message);
        }

        [Test]
        public void when_applying_a_promo_that_expired()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                IsFutureBooking = false,
                PickupDate = new DateTime(2016, 11, 21, 13, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionHasExpired", ex.Message);
        }

        [Test]
        public void when_applying_a_promo_at_a_time_of_the_day_that_it_is_not_active()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _sut.When(new ApplyPromotion
            {
                PromoId = _promoId,
                AccountId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                IsFutureBooking = false,
                PickupDate = new DateTime(2014, 11, 24, 9, 0, 0)
            }));
            Assert.AreEqual("CannotCreateOrder_PromotionNotAvailableAtThisTime", ex.Message);
        }
    }
}