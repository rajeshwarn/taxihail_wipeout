using System;
using System.Data.SqlTypes;
using System.Linq;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.PromotionFixture
{
    [TestFixture]
    public class given_one_promotion
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Domain.Promotion>();

            _sut.Setup(new PromotionCommandHandler(_sut.Repository));
            _sut.Given(new PromotionCreated
            {
                SourceId = _promoId,
                Name = "promo1",
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
                EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0)
            });
        }

        private EventSourcingTestHelper<Domain.Promotion> _sut;
        private Guid _promoId = Guid.NewGuid();

        [Test]
        public void when_updating_promo_successfully()
        {
            _sut.When(new UpdatePromotion
            {
                PromoId = _promoId, 
                Name = "promo2", 
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
        }

        [Test]
        public void when_updating_a_promo_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.When(new UpdatePromotion { PromoId = _promoId }));
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
    }
}