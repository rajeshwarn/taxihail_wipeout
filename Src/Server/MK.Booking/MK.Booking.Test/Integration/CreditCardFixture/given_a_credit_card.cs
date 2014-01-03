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
    public class given_a_credit_card : given_a_view_model_generator
    {
        [SetUp]
        public void Setup()
        {
            _accountId = Guid.NewGuid();
            const string creditCardComapny = "visa";
            const string friendlyName = "work credit card";
            _creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            Sut.Handle(new CreditCardAdded
            {
                SourceId = _accountId,
                CreditCardCompany = creditCardComapny,
                FriendlyName = friendlyName,
                CreditCardId = _creditCardId,
                Last4Digits = last4Digits,
                Token = token
            });
        }

        private Guid _creditCardId;
        private Guid _accountId;

        [Test]
        public void when_all_cc_are_deleted()
        {
            Sut.Handle(new AllCreditCardsRemoved
            {
                SourceId = _accountId,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var address = context.Find<CreditCardDetails>(_creditCardId);
                Assert.IsNull(address);
            }
        }

        [Test]
        public void when_creditcard_is_removed_then_list_updated()
        {
            Sut.Handle(new CreditCardRemoved
            {
                SourceId = _accountId,
                CreditCardId = _creditCardId
            });

            using (var context = new BookingDbContext(DbName))
            {
                var address = context.Find<CreditCardDetails>(_creditCardId);
                Assert.IsNull(address);
            }
        }
    }
}