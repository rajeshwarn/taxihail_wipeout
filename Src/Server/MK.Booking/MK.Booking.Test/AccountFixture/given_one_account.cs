using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Events;
using Xunit;

namespace apcurium.MK.Booking.Test.OrganizationFixture
{
    public class given_one_account
    {

        private EventSourcingTestHelper<Account> sut;
        private Guid _accountId = Guid.NewGuid();

        public given_one_account()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.sut.Setup(new AccountCommandHandler(this.sut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, FirstName = "Bob", LastName = "Smith", Password = "bsmith", Email = "bob.smith@apcurium.com" });
        }

        [Fact]
        public void when_updating_successfully()
        {
            this.sut.When(new UpdateAccount { AccountId = _accountId, FirstName = "Robert", LastName = "Smither" });

            var @event = sut.ThenHasSingle<AccountUpdated>();

            Assert.Equal(_accountId, @event.SourceId);
            Assert.Equal("Robert", @event.FirstName);
            Assert.Equal("Smither", @event.LastName);
            
        }
    }
}
