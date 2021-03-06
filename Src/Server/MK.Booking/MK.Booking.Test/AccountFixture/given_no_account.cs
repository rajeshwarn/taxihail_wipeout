﻿#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_no_account
    {
        [SetUp]
        public void given_no_account_setup()
        {
            _sut = new EventSourcingTestHelper<Account>();
            _sut.Setup(new AccountCommandHandler(_sut.Repository, new PasswordService(), null));
        }

        private EventSourcingTestHelper<Account> _sut;
        private readonly Guid _accountId = Guid.NewGuid();

        [Test]
        public void when_registering_account_successfully()
        {
            _sut.When(new RegisterAccount
            {
                AccountId = _accountId,
                Name = "Bob Smith",
                Phone = "5145551111",
                Country = new CountryISOCode("CA"),
                Password = "bsmith",
                Email = "bob.smith@apcurium.com",
                Language = "fr",
                ConfimationToken = "token"
            });

            var @event = _sut.ThenHasSingle<AccountRegistered>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Bob Smith", @event.Name);
            Assert.IsNotEmpty(@event.Password);
            Assert.AreEqual("bob.smith@apcurium.com", @event.Email);
            Assert.AreEqual("5145551111", @event.Phone);
            Assert.AreEqual("CA", @event.Country.Code);
            Assert.AreEqual("fr", @event.Language);
        }

        [Test]
        public void when_registering_account_successfully_when_account_activation_disabled()
        {
            _sut.When(new RegisterAccount
            {
                AccountId = _accountId,
                Name = "Bob Smith",
                Phone = "5145551111",
                Country = new CountryISOCode("CA"),
                Password = "bsmith",
                Email = "bob.smith@apcurium.com",
                Language = "fr",
                ConfimationToken = "token",
                AccountActivationDisabled = true
            });

            var @event = _sut.ThenHasSingle<AccountRegistered>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Bob Smith", @event.Name);
            Assert.IsNotEmpty(@event.Password);
            Assert.AreEqual("bob.smith@apcurium.com", @event.Email);
            Assert.AreEqual("5145551111", @event.Phone);
            Assert.AreEqual("CA", @event.Country.Code);
            Assert.AreEqual("fr", @event.Language);
            Assert.AreEqual(true, @event.AccountActivationDisabled);
        }

        [Test]
        public void when_registering_account_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(
                () => _sut.When(new RegisterAccount {AccountId = _accountId, Name = "Bob"}));
        }

        [Test]
        public void when_registering_facebook_account_successfully()
        {
            _sut.When(new RegisterFacebookAccount
            {
                AccountId = _accountId,
                Name = "Francois Cuvelier",
                Phone = "5145551111",
                Country = new CountryISOCode("CA"),
                Email = "francois.cuvelier@apcurium.com",
                FacebookId = "123456789",
                Language = "fr"
            });

            var @event = _sut.ThenHasSingle<AccountRegistered>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Francois Cuvelier", @event.Name);
            Assert.AreEqual("francois.cuvelier@apcurium.com", @event.Email);
            Assert.AreEqual("5145551111", @event.Phone);
            Assert.AreEqual("CA", @event.Country.Code);
            Assert.AreEqual("123456789", @event.FacebookId);
            Assert.AreEqual("fr", @event.Language);
        }

        [Test]
        public void when_registering_twitter_account_successfully()
        {
            _sut.When(new RegisterTwitterAccount
            {
                AccountId = _accountId,
                Name = "Francois Cuvelier",
                Phone = "5145551111",
                Country = new CountryISOCode("CA"),
                Email = "francois.cuvelier@apcurium.com",
                TwitterId = "123456789",
                Language = "fr"
            });

            var @event = _sut.ThenHasSingle<AccountRegistered>();
            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Francois Cuvelier", @event.Name);
            Assert.AreEqual("francois.cuvelier@apcurium.com", @event.Email);
            Assert.AreEqual("5145551111", @event.Phone);
            Assert.AreEqual("CA", @event.Country.Code);
            Assert.AreEqual("123456789", @event.TwitterId);
            Assert.AreEqual("fr", @event.Language);
        }
    }
}