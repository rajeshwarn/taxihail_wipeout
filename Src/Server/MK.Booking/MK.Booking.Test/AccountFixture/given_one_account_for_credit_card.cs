using System;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_one_account_for_credit_card
    {
        private EventSourcingTestHelper<Account> sut;
        private readonly Guid _accountId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Account>();

            this.sut.Setup(new AccountCommandHandler(this.sut.Repository, new PasswordService()));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, Name = "Bob", Password = null, Email = "bob.smith@apcurium.com", IbsAcccountId = 10, ConfirmationToken = "token" });
        }

        [Test]
        public void when_add_new_credit_card()
        {
            const string creditCardComapny = "visa";
            const string friendlyName = "work credit card";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            this.sut.When(new AddCreditCard { AccountId = _accountId, CreditCardCompany = creditCardComapny, FriendlyName = friendlyName, CreditCardId = creditCardId, Last4Digits = last4Digits, Token = token, Id = Guid.NewGuid()});

            var @event = sut.ThenHasSingle<CreditCardAdded>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(creditCardComapny, @event.CreditCardCompany);
            Assert.AreEqual(friendlyName, @event.FriendlyName);
            Assert.AreEqual(creditCardId, @event.CreditCardId);
            Assert.AreEqual(last4Digits, @event.Last4Digits);
            Assert.AreEqual(token, @event.Token);
        }

        [Test]
        public void when_remove_credit_card()
        {
            var creditCardId = Guid.NewGuid();

            this.sut.When(new RemoveCreditCard { AccountId = _accountId, CreditCardId = creditCardId });

            var @event = sut.ThenHasSingle<CreditCardRemoved>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(creditCardId, @event.CreditCardId);
        }

        [Test]
        public void when_updating_paynment_profile()
        {
            Guid? creditCardId = Guid.NewGuid();
            double? tipAmount = 10.0;
            double? defaultTipPercent = 15.0;

            this.sut.When(new UpdatePaymentProfile{ AccountId =  _accountId, DefaultCreditCard = creditCardId, DefaultTipAmount = tipAmount, DefaultTipPercent = defaultTipPercent });

            var @event = sut.ThenHasSingle<PaymentProfileUpdated>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(creditCardId, @event.DefaultCreditCard);
            Assert.AreEqual(tipAmount, @event.DefaultTipAmount);
            Assert.AreEqual(defaultTipPercent, @event.DefaultTipPercent);
        }
    }
}