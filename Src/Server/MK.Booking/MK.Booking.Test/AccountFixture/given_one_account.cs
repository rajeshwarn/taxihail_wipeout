using System;
using System.Net.Mail;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_one_account
    {

        private EventSourcingTestHelper<Account> sut;
        private Guid _accountId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Account>();

            this.sut.Setup(new AccountCommandHandler(this.sut.Repository, new PasswordService()));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, FirstName = "Bob", LastName = "Smith", Password = null, Email = "bob.smith@apcurium.com", IbsAcccountId=10 });
        }

        [Test]
        public void when_updating_successfully()
        {
            this.sut.When(new UpdateAccount { AccountId = _accountId, FirstName = "Robert", LastName = "Smither" });

            var @event = sut.ThenHasSingle<AccountUpdated>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Robert", @event.FirstName);
            Assert.AreEqual("Smither", @event.LastName);
            
        }

        [Test]
        public void when_reseting_password_successfully()
        {
            this.sut.When(new ResetAccountPassword { AccountId = _accountId, Password = "Yop" });

            var @event = sut.ThenHasSingle<AccountPasswordResetted>();

            Assert.AreEqual(_accountId, @event.SourceId);

            var service = new PasswordService();

            Assert.AreEqual(true, service.IsValid("Yop", _accountId.ToString(), @event.Password));

        }

    }
}
