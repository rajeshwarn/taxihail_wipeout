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

        public given_no_account()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.sut.Setup(new AccountCommandHandler(this.sut.Repository, new PasswordService()));
        }

        [Test]
        public void when_registering_account_successfully()
        {
            this.sut.When(new RegisterAccount { AccountId = _accountId, FirstName = "Bob", LastName= "Smith", Phone = "888", Password = "bsmith", Email = "bob.smith@apcurium.com", IbsAccountId = 999});

            Assert.AreEqual(1, sut.Events.Count);
            Assert.AreEqual(_accountId, ((AccountRegistered)sut.Events.Single()).SourceId);
            Assert.AreEqual("Bob", ((AccountRegistered)sut.Events.Single()).FirstName);
            Assert.AreEqual("Smith", ((AccountRegistered)sut.Events.Single()).LastName);
            Assert.IsNotEmpty(((AccountRegistered)sut.Events.Single()).Password);
            Assert.AreEqual("bob.smith@apcurium.com", ((AccountRegistered)sut.Events.Single()).Email);
            Assert.AreEqual("888", ((AccountRegistered)sut.Events.Single()).Phone);
            Assert.AreEqual(999, ((AccountRegistered)sut.Events.Single()).IbsAcccountId);

        }

        [Test]
        public void when_registering_account_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new RegisterAccount { AccountId = _accountId, FirstName = "Bob" }));
        }
    }
}
