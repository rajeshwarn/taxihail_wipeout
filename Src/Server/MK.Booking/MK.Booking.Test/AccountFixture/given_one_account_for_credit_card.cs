﻿#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
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

            _sut.Setup(new AccountCommandHandler(_sut.Repository, new PasswordService(), null, new TestServerSettings()));
            _sut.Given(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob",
                Password = null,
                Email = "bob.smith@apcurium.com",
                ConfirmationToken = "token"
            });
        }

        private EventSourcingTestHelper<Account> _sut;
        private readonly Guid _accountId = Guid.NewGuid();

        [Test]
        public void when_add_first_credit_card()
        {
            const string creditCardCompany = "visa";
            const string nameOnCard = "Bob";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string expirationMonth = "5";
            const string expirationYear = "2020";
            const string token = "jjwcnSLWm85";

            _sut.When(new AddOrUpdateCreditCard
            {
                AccountId = _accountId,
                CreditCardCompany = creditCardCompany,
                NameOnCard = nameOnCard,
                CreditCardId = creditCardId,
                Last4Digits = last4Digits,
                ExpirationMonth = expirationMonth,
                ExpirationYear = expirationYear,
                Token = token,
                Id = Guid.NewGuid()
            });

            var @event = _sut.ThenHasOne<CreditCardAddedOrUpdated>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(creditCardCompany, @event.CreditCardCompany);
            Assert.AreEqual(nameOnCard, @event.NameOnCard);
            Assert.AreEqual(creditCardId, @event.CreditCardId);
            Assert.AreEqual(last4Digits, @event.Last4Digits);
            Assert.AreEqual(expirationMonth, @event.ExpirationMonth);
            Assert.AreEqual(expirationYear, @event.ExpirationYear);
            Assert.AreEqual(token, @event.Token);
        }

        [Test]
        public void when_updating_credit_card()
        {
            const string creditCardCompany = "mastercard";
            const string nameOnCard = "Bob 2";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string expirationMonth = "5";
            const string expirationYear = "2020";
            const string token = "jjwcnSLWm85";

            _sut.Given(new CreditCardAddedOrUpdated { SourceId = _accountId, CreditCardId = creditCardId });

            _sut.When(new AddOrUpdateCreditCard
            {
                AccountId = _accountId,
                CreditCardCompany = creditCardCompany,
                CreditCardId = creditCardId,
                NameOnCard = nameOnCard,
                Last4Digits = last4Digits,
                ExpirationMonth = expirationMonth,
                ExpirationYear = expirationYear,
                Token = token,
                Id = Guid.NewGuid()
            });

            var @event = _sut.ThenHasSingle<CreditCardAddedOrUpdated>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(creditCardCompany, @event.CreditCardCompany);
            Assert.AreEqual(nameOnCard, @event.NameOnCard);
            Assert.AreEqual(creditCardId, @event.CreditCardId);
            Assert.AreEqual(last4Digits, @event.Last4Digits);
            Assert.AreEqual(expirationMonth, @event.ExpirationMonth);
            Assert.AreEqual(expirationYear, @event.ExpirationYear);
            Assert.AreEqual(token, @event.Token);
        }

        [Test]
        public void when_remove_all_credit_cards()
        {
            _sut.When(new DeleteAccountCreditCards {AccountId = _accountId});

            var @event = _sut.ThenHasSingle<AllCreditCardsRemoved>();
            Assert.AreEqual(_accountId, @event.SourceId);
        }

        [Test]
        public void when_credit_card_deactivated()
        {
            var orderId = Guid.NewGuid();

            _sut.When(new ReactToPaymentFailure
            {
                AccountId = _accountId,
                OrderId = orderId,
                OverdueAmount = 12.56m
            });

            var creditCardDeactivated = _sut.ThenHasOne<CreditCardDeactivated>();
            Assert.AreEqual(_accountId, creditCardDeactivated.SourceId);
            Assert.AreEqual(true, creditCardDeactivated.IsOutOfAppPaymentDisabled);

            var overduePaymentLogged = _sut.ThenHasOne<OverduePaymentLogged>();
            Assert.AreEqual(_accountId, overduePaymentLogged.SourceId);
        }
    }
}