using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Test.Integration.CreditCardPaymentFixture
{
    public class given_a_payment : given_a_view_model_generator
    {
        private Guid _paymentId;
        [SetUp]
        public void Setup()
        {
            _paymentId = Guid.NewGuid();
            Sut.Handle(new CreditCardPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = Guid.NewGuid(),
                Amount = 12.34m,
                TransactionId = "the transaction",
            });
        }


        [Test]
        public void when_payment_captured_then_dto_updated()
        {
            Sut.Handle(new CreditCardPaymentCaptured
            {
                SourceId = _paymentId,
            });

            using (var context = new BookingDbContext(dbName))
            {
                var dto = context.Find<OrderPaymentDetail>(_paymentId);
                Assert.NotNull(dto);
                Assert.AreEqual(true, dto.IsCompleted);
            }
        }
    }
}
