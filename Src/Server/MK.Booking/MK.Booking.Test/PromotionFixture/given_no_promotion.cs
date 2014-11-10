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
    public class given_no_promotion
    {
        [SetUp]
        public void given_no_promotion_setup()
        {
            _sut = new EventSourcingTestHelper<Domain.Promotion>();
            _sut.Setup(new PromotionCommandHandler(_sut.Repository));
        }

        private EventSourcingTestHelper<Domain.Promotion> _sut;
        private readonly Guid _promoId = Guid.NewGuid();

        [Test]
        public void when_creating_a_promo_successfully()
        {
            _sut.When(new CreatePromotion
            {
                PromoId = _promoId,
                Name = "promo1",
                Code = "code",
                AppliesToCurrentBooking = true,
                AppliesToFutureBooking = false,
                DiscountType = PromoDiscountType.Percentage,
                DiscountValue = 10,
                DaysOfWeek = new [] { DayOfWeek.Monday, DayOfWeek.Tuesday },
                MaxUsage = 2,
                MaxUsagePerUser = 1,
                StartDate = new DateTime(2014, 11, 10),
                EndDate = new DateTime(2015, 11, 10),
                StartTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 10, 0, 0),
                EndTime = new DateTime(SqlDateTime.MinValue.Value.Year, SqlDateTime.MinValue.Value.Month, SqlDateTime.MinValue.Value.Day, 14, 0, 0)
            });

            var @event = _sut.ThenHasSingle<PromotionCreated>();
            Assert.AreEqual(_promoId, @event.SourceId);
            Assert.AreEqual("promo1", @event.Name);
            Assert.AreEqual("code", @event.Code);
            Assert.AreEqual(true, @event.AppliesToCurrentBooking);
            Assert.AreEqual(false, @event.AppliesToFutureBooking);
            Assert.AreEqual(PromoDiscountType.Percentage, @event.DiscountType);
            Assert.AreEqual(10, @event.DiscountValue);
            Assert.AreEqual(2, @event.MaxUsage);
            Assert.AreEqual(1, @event.MaxUsagePerUser);
            Assert.AreEqual(2, @event.DaysOfWeek.Count());
            Assert.AreEqual(new DateTime(2014, 11, 10), @event.StartDate);
            Assert.AreEqual(new DateTime(2015, 11, 10), @event.EndDate);
            Assert.AreEqual(SqlDateTime.MinValue.Value.Date, @event.StartTime.Value.Date);
            Assert.AreEqual(SqlDateTime.MinValue.Value.Date, @event.EndTime.Value.Date);
            Assert.AreEqual(new TimeSpan(10, 0, 0), @event.StartTime.Value.TimeOfDay);
            Assert.AreEqual(new TimeSpan(14, 0, 0), @event.EndTime.Value.TimeOfDay);
        }

        [Test]
        public void when_creating_a_promo_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.When(new CreatePromotion {PromoId = _promoId}));
        }
    }
}