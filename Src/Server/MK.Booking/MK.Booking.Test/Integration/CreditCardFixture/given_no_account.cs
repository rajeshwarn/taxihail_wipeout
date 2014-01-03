#region

using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration.CreditCardFixture
{
    [TestFixture]
    public class given_no_account : given_a_view_model_generator
    {
        [Test]
        public void when_creditcard_added_then_creditcard_dto_populated()
        {
            var accountId = Guid.NewGuid();
            const string creditCardComapny = "visa";
            const string friendlyName = "work credit card";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            Sut.Handle(new CreditCardAdded
            {
                SourceId = accountId,
                CreditCardCompany = creditCardComapny,
                FriendlyName = friendlyName,
                CreditCardId = creditCardId,
                Last4Digits = last4Digits,
                Token = token
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<CreditCardDetails>(creditCardId);
                Assert.NotNull(dto);
                Assert.AreEqual(accountId, dto.AccountId);
                Assert.AreEqual(creditCardComapny, dto.CreditCardCompany);
                Assert.AreEqual(friendlyName, dto.FriendlyName);
                Assert.AreEqual(creditCardId, dto.CreditCardId);
                Assert.AreEqual(last4Digits, dto.Last4Digits);
                Assert.AreEqual(token, dto.Token);
            }
        }
    }
}