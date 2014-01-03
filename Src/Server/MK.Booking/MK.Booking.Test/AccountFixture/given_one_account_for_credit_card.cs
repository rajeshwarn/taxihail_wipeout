#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_one_account_for_credit_card
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Account>();

            _sut.Setup(new AccountCommandHandler(_sut.Repository, new PasswordService()));
            _sut.Given(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob",
                Password = null,
                Email = "bob.smith@apcurium.com",
                IbsAcccountId = 10,
                ConfirmationToken = "token"
            });
        }

        private EventSourcingTestHelper<Account> _sut;
        private readonly Guid _accountId = Guid.NewGuid();

        [Test]
        public void when_add_first_credit_card()
        {
            const string creditCardCompany = "visa";
            const string friendlyName = "work credit card";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            _sut.When(new AddCreditCard
            {
                AccountId = _accountId,
                CreditCardCompany = creditCardCompany,
                FriendlyName = friendlyName,
                CreditCardId = creditCardId,
                Last4Digits = last4Digits,
                Token = token,
                Id = Guid.NewGuid()
            });

            var @event = _sut.ThenHasOne<CreditCardAdded>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(creditCardCompany, @event.CreditCardCompany);
            Assert.AreEqual(friendlyName, @event.FriendlyName);
            Assert.AreEqual(creditCardId, @event.CreditCardId);
            Assert.AreEqual(last4Digits, @event.Last4Digits);
            Assert.AreEqual(token, @event.Token);

            var secondEvent = _sut.ThenHasOne<PaymentProfileUpdated>();
            Assert.AreEqual(_accountId, secondEvent.SourceId);
            Assert.AreEqual(creditCardId, secondEvent.DefaultCreditCard);
        }

        [Test]
        public void when_add_second_credit_card()
        {
            const string creditCardCompany = "visa";
            const string friendlyName = "work credit card";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            _sut.Given(new CreditCardAdded {SourceId = _accountId});

            _sut.When(new AddCreditCard
            {
                AccountId = _accountId,
                CreditCardCompany = creditCardCompany,
                FriendlyName = friendlyName,
                CreditCardId = creditCardId,
                Last4Digits = last4Digits,
                Token = token,
                Id = Guid.NewGuid()
            });

            var @event = _sut.ThenHasSingle<CreditCardAdded>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(creditCardCompany, @event.CreditCardCompany);
            Assert.AreEqual(friendlyName, @event.FriendlyName);
            Assert.AreEqual(creditCardId, @event.CreditCardId);
            Assert.AreEqual(last4Digits, @event.Last4Digits);
            Assert.AreEqual(token, @event.Token);
        }

        [Test]
        public void when_remove_credit_card()
        {
            var creditCardId = Guid.NewGuid();

            _sut.When(new RemoveCreditCard {AccountId = _accountId, CreditCardId = creditCardId});

            var @event = _sut.ThenHasSingle<CreditCardRemoved>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(creditCardId, @event.CreditCardId);
        }
    }
}