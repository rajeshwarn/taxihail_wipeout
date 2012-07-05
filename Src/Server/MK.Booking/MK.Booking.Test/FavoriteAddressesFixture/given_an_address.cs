using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;

namespace BackOffice.Test.FavoriteAddressesFixture
{
    public class given_an_address
    {
        private EventSourcingTestHelper<Account> sut;
        private readonly Guid _accountId = Guid.NewGuid();
        private readonly Guid _addressId = Guid.NewGuid();

        public given_an_address()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.sut.Setup(new FavoriteAddressCommandHandler(this.sut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, FirstName = "Bob", LastName = "Smith", Password = "bsmith", Email = "bob.smith@apcurium.com" });
            this.sut.Given(new FavoriteAddressAdded { AddressId = _addressId, SourceId = _accountId, FriendlyName = "Chez François", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 });
        }

        [Fact]
        public void when_address_removed_successfully()
        {
            this.sut.When(new RemoveFavoriteAddress { AccountId = _accountId, AddressId = _addressId });

            Assert.Single(sut.Events);
            var evt = (FavoriteAddressRemoved) sut.Events.Single();
            Assert.Equal(_accountId, evt.SourceId);
            Assert.Equal(_addressId, evt.AddressId);
        }

        [Fact]
        public void when_removing_unknown_address()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new RemoveFavoriteAddress
            {
                AccountId = _accountId,
                AddressId = Guid.NewGuid()
            }));
        }

        [Fact]
        public void when_address_updated_successfully()
        {
            this.sut.When(new UpdateFavoriteAddress { AccountId = _accountId, AddressId = _addressId, FriendlyName = "Chez Costo", FullAddress = "1234 rue Saint-Hubert" });

            Assert.Single(sut.Events);
            var evt = (FavoriteAddressUpdated)sut.Events.Single();
            Assert.Equal(_accountId, evt.SourceId);
            Assert.Equal(_addressId, evt.AddressId);
        }

        [Fact]
        public void when_address_updated_with_missing_value()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new UpdateFavoriteAddress { AccountId = _accountId, AddressId = _addressId, FriendlyName = "Chez Costo"}));
        }
    }
}
