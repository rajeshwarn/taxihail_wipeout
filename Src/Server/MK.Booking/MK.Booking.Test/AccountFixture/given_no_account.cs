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

            Assert.AreEqual(1, sut.Events.Count);
            Assert.AreEqual(_accountId, ((AccountRegistered)sut.Events.Single()).SourceId);
            Assert.AreEqual("Bob Smith", ((AccountRegistered)sut.Events.Single()).Name);            
            Assert.IsNotEmpty(((AccountRegistered)sut.Events.Single()).Password);
            Assert.AreEqual("bob.smith@apcurium.com", ((AccountRegistered)sut.Events.Single()).Email);
            Assert.AreEqual("888", ((AccountRegistered)sut.Events.Single()).Phone);
            Assert.AreEqual(999, ((AccountRegistered)sut.Events.Single()).IbsAcccountId);
            Assert.AreEqual("fr", ((AccountRegistered)sut.Events.Single()).Language);
        }

        [Test]
        public void when_registering_account_successfully_when_account_activation_disabled()
        {
            this.sut.When(new RegisterAccount { AccountId = _accountId, Name = "Bob Smith", Phone = "888", Password = "bsmith", Email = "bob.smith@apcurium.com", IbsAccountId = 999, Language = "fr", ConfimationToken = "token", AccountActivationDisabled = true});

            Assert.AreEqual(1, sut.Events.Count);
            Assert.AreEqual(_accountId, ((AccountRegistered)sut.Events.Single()).SourceId);
            Assert.AreEqual("Bob Smith", ((AccountRegistered)sut.Events.Single()).Name);
            Assert.IsNotEmpty(((AccountRegistered)sut.Events.Single()).Password);
            Assert.AreEqual("bob.smith@apcurium.com", ((AccountRegistered)sut.Events.Single()).Email);
            Assert.AreEqual("888", ((AccountRegistered)sut.Events.Single()).Phone);
            Assert.AreEqual(999, ((AccountRegistered)sut.Events.Single()).IbsAcccountId);
            Assert.AreEqual("fr", ((AccountRegistered)sut.Events.Single()).Language);
            Assert.AreEqual(true, ((AccountRegistered)sut.Events.Single()).AccountActivationDisabled);
        }

        [Test]
        public void when_registering_facebook_account_successfully()
        {
            this.sut.When(new RegisterFacebookAccount() { AccountId = _accountId, Name = "Francois Cuvelier", Phone = "888",   Email = "francois.cuvelier@apcurium.com", IbsAccountId = 999, FacebookId = "123456789", Language = "fr"});

            Assert.AreEqual(1, sut.Events.Count);
            Assert.AreEqual(_accountId, ((AccountRegistered)sut.Events.Single()).SourceId);
            Assert.AreEqual("Francois Cuvelier", ((AccountRegistered)sut.Events.Single()).Name);
            Assert.AreEqual("francois.cuvelier@apcurium.com", ((AccountRegistered)sut.Events.Single()).Email);
            Assert.AreEqual("888", ((AccountRegistered)sut.Events.Single()).Phone);
            Assert.AreEqual(999, ((AccountRegistered)sut.Events.Single()).IbsAcccountId);
            Assert.AreEqual("123456789", ((AccountRegistered)sut.Events.Single()).FacebookId);
            Assert.AreEqual("fr", ((AccountRegistered)sut.Events.Single()).Language);

        }

        [Test]
        public void when_registering_twitter_account_successfully()
        {
            this.sut.When(new RegisterTwitterAccount() { AccountId = _accountId, Name = "Francois Cuvelier", Phone = "888", Email = "francois.cuvelier@apcurium.com", IbsAccountId = 999, TwitterId = "123456789", Language = "fr"});

            Assert.AreEqual(1, sut.Events.Count);
            Assert.AreEqual(_accountId, ((AccountRegistered)sut.Events.Single()).SourceId);
            Assert.AreEqual("Francois Cuvelier", ((AccountRegistered)sut.Events.Single()).Name);
            Assert.AreEqual("francois.cuvelier@apcurium.com", ((AccountRegistered)sut.Events.Single()).Email);
            Assert.AreEqual("888", ((AccountRegistered)sut.Events.Single()).Phone);
            Assert.AreEqual(999, ((AccountRegistered)sut.Events.Single()).IbsAcccountId);
            Assert.AreEqual("123456789", ((AccountRegistered)sut.Events.Single()).TwitterId);
            Assert.AreEqual("fr", ((AccountRegistered)sut.Events.Single()).Language);

        }

        [Test]
        public void when_registering_account_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new RegisterAccount { AccountId = _accountId, Name = "Bob" }));
        }
    }
}
