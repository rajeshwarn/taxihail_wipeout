#region

using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration.CreditCardPaymentFixture
{
    public class given_a_payment : given_a_view_model_generator
    {
        private Guid _paymentId;

        [SetUp]
        public void Setup()
        {
            _paymentId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            Sut.Handle(new CreditCardPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = orderId,
                Amount = 12.34m,
                TransactionId = "the transaction",
            });
            var ordetailsGenerator = new OrderGenerator(() => new BookingDbContext(DbName), new Logger());
            ordetailsGenerator.Handle(new OrderCreated
            {
                SourceId = orderId,
                AccountId = Guid.NewGuid(),
                PickupAddress = new Address
                {
                    Apartment = "3939",
                    Street = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                },
                PickupDate = DateTime.Now,
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
        public void when_payment_captured_then_dto_updated()
        {
            Sut.Handle(new CreditCardPaymentCaptured
            {
                SourceId = _paymentId,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderPaymentDetail>(_paymentId);
                Assert.NotNull(dto);
                Assert.AreEqual(true, dto.IsCompleted);
            }
        }
    }
}