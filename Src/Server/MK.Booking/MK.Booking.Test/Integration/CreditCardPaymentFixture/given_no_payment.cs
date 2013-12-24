#region

using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration.CreditCardPaymentFixture
{
    [TestFixture]
    public class given_no_payment : given_a_view_model_generator
    {
        [Test]
        public void when_payment_initiated_then_dto_populated()
        {
            var paymentId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var t = Guid.NewGuid().ToString();
            Sut.Handle(new CreditCardPaymentInitiated
            {
                SourceId = paymentId,
                Amount = 34.56m,
                TransactionId = "the transaction",
                OrderId = orderId,
                CardToken = t,
                Provider = PaymentProvider.Braintree
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderPaymentDetail>(paymentId);
                Assert.NotNull(dto);
                Assert.AreEqual(orderId, dto.OrderId);
                Assert.AreEqual(34.56m, dto.Amount);
                Assert.AreEqual("the transaction", dto.TransactionId);
                Assert.AreEqual(false, dto.IsCompleted);
                Assert.AreEqual(t, dto.CardToken);
                Assert.AreEqual(PaymentProvider.Braintree, dto.Provider);
                Assert.AreEqual(PaymentType.CreditCard, dto.Type);
            }
        }
    }
}