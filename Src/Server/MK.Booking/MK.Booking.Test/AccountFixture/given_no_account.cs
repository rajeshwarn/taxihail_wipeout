using System;
using System.Linq;
using NUnit.Framework;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_no_account
    {
        private EventSourcingTestHelper<Account> sut;
        private Guid _accountId = Guid.NewGuid();

        [SetUp]
        public void given_no_account_setup()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.sut.Setup(new AccountCommandHandler(this.sut.Repository, new PasswordService()));
        }

        [Test]
        public void when_registering_account_successfully()
        {
            this.sut.When(new RegisterAccount { AccountId = _accountId, Name = "Bob Smith", Phone = "888", Password = "bsmith", Email = "bob.smith@apcurium.com", IbsAccountId = 999, Language = "fr", ConfimationToken = "token"});

            var @event = sut.ThenHasSingle<AccountRegistered>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Bob Smith", @event.Name);            
            Assert.IsNotEmpty(@event.Password);
            Assert.AreEqual("bob.smith@apcurium.com", @event.Email);
            Assert.AreEqual("888", @event.Phone);
            Assert.AreEqual(999, @event.IbsAcccountId);
            Assert.AreEqual("fr", @event.Language);
        }

        [Test]
        public void when_registering_account_successfully_when_account_activation_disabled()
        {
            this.sut.When(new RegisterAccount { AccountId = _accountId, Name = "Bob Smith", Phone = "888", Password = "bsmith", Email = "bob.smith@apcurium.com", IbsAccountId = 999, Language = "fr", ConfimationToken = "token", AccountActivationDisabled = true});

            var @event = sut.ThenHasSingle<AccountRegistered>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Bob Smith", @event.Name);
            Assert.IsNotEmpty(@event.Password);
            Assert.AreEqual("bob.smith@apcurium.com", @event.Email);
            Assert.AreEqual("888", @event.Phone);
            Assert.AreEqual(999, @event.IbsAcccountId);
            Assert.AreEqual("fr", @event.Language);
            Assert.AreEqual(true, @event.AccountActivationDisabled);
        }

        [Test]
        public void when_registering_facebook_account_successfully()
        {
            this.sut.When(new RegisterFacebookAccount() { AccountId = _accountId, Name = "Francois Cuvelier", Phone = "888",   Email = "francois.cuvelier@apcurium.com", IbsAccountId = 999, FacebookId = "123456789", Language = "fr"});

            var @event = sut.ThenHasSingle<AccountRegistered>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Francois Cuvelier", @event.Name);
            Assert.AreEqual("francois.cuvelier@apcurium.com", @event.Email);
            Assert.AreEqual("888", @event.Phone);
            Assert.AreEqual(999, @event.IbsAcccountId);
            Assert.AreEqual("123456789", @event.FacebookId);
            Assert.AreEqual("fr", @event.Language);

        }

        [Test]
        public void when_registering_twitter_account_successfully()
        {
            this.sut.When(new RegisterTwitterAccount() { AccountId = _accountId, Name = "Francois Cuvelier", Phone = "888", Email = "francois.cuvelier@apcurium.com", IbsAccountId = 999, TwitterId = "123456789", Language = "fr"});

            var @event = sut.ThenHasSingle<AccountRegistered>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Francois Cuvelier", @event.Name);
            Assert.AreEqual("francois.cuvelier@apcurium.com", @event.Email);
            Assert.AreEqual("888", @event.Phone);
            Assert.AreEqual(999, @event.IbsAcccountId);
            Assert.AreEqual("123456789", @event.TwitterId);
            Assert.AreEqual("fr", @event.Language);

        }

        [Test]
        public void when_registering_account_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new RegisterAccount { AccountId = _accountId, Name = "Bob" }));
        }
    }
}
