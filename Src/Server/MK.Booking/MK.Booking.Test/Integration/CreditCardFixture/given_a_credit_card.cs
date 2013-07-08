using System;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Test.Integration.CreditCardFixture
{
    [TestFixture]
    public class given_a_credit_card : given_a_view_model_generator
    {
        private Guid _creditCardId;
        private Guid _accountId;

        [SetUp]
        public void Setup()
        {
            _accountId = Guid.NewGuid();
            const string creditCardComapny = "visa";
            const string friendlyName = "work credit card";
            _creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            this.sut.Handle(new CreditCardAdded
                {
                    SourceId = _accountId,
                    CreditCardCompany = creditCardComapny,
                    FriendlyName = friendlyName,
                    CreditCardId = _creditCardId,
                    Last4Digits = last4Digits,
                    Token = token
                });
        }

        [Test]
        public void when_creditcard_is_removed_then_list_updated()
        {
            this.sut.Handle(new CreditCardRemoved
                {
                    SourceId = _accountId,
                    CreditCardId = _creditCardId
                });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<CreditCardDetails>(_creditCardId);
                Assert.IsNull(address);
            }
        }

        [Test]
        public void when_all_cc_are_deleted()
        {
            sut.Handle(new AllCreditCardsRemoved()
            {
                SourceId = _accountId,
            });

            using (var context = new BookingDbContext(dbName))
            {
                var address = context.Find<CreditCardDetails>(_creditCardId);
                Assert.IsNull(address);
            }
        }

    }
}