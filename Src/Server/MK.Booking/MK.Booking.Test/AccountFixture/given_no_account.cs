using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Common.Tests;
using Xunit;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.Test.OrganizationFixture
{
    public class given_no_account
    {
        private EventSourcingTestHelper<Account> sut;
        private Guid _accountId = Guid.NewGuid();

        public given_no_account()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.sut.Setup(new AccountCommandHandler(this.sut.Repository));
        }

        [Fact]
        public void when_registering_account_successfully()
        {
            this.sut.When(new RegisterAccount { AccountId = _accountId, FirstName = "Bob", LastName= "Smith", Phone = "888", Password = "bsmith", Email = "bob.smith@apcurium.com" });

            Assert.Single(sut.Events);
            Assert.Equal(_accountId, ((AccountRegistered)sut.Events.Single()).SourceId);
            Assert.Equal("Bob", ((AccountRegistered)sut.Events.Single()).FirstName);
            Assert.Equal("Smith", ((AccountRegistered)sut.Events.Single()).LastName);
            Assert.Equal("bsmith", ((AccountRegistered)sut.Events.Single()).Password);
            Assert.Equal("bob.smith@apcurium.com", ((AccountRegistered)sut.Events.Single()).Email);
            Assert.Equal("888", ((AccountRegistered)sut.Events.Single()).Phone);

        }

        [Fact]
        public void when_registering_account_with_missing_required_fields()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new RegisterAccount { AccountId = _accountId, FirstName = "Bob" }));
        }
    }
}
