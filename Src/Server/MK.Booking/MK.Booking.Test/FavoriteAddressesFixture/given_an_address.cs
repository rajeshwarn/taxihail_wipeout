using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.Test.FavoriteAddressesFixture
{
    [TestFixture]
    public class given_an_address
    {
        private EventSourcingTestHelper<Account> sut;
        private readonly Guid _accountId = Guid.NewGuid();
        private readonly Guid _addressId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.sut.Setup(new AddressCommandHandler(this.sut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, Name = "Bob", Password = null, Email = "bob.smith@apcurium.com" });
            this.sut.Given(new FavoriteAddressAdded { AddressId = _addressId, SourceId = _accountId, FriendlyName = "Chez François", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 });
        }

        [Test]
        public void when_address_removed_successfully()
        {
            this.sut.When(new RemoveFavoriteAddress { AccountId = _accountId, AddressId = _addressId });

            Assert.AreEqual(1, sut.Events.Count);
            var evt = (FavoriteAddressRemoved) sut.Events.Single();
            Assert.AreEqual(_accountId, evt.SourceId);
            Assert.AreEqual(_addressId, evt.AddressId);
        }

        [Test]
        public void when_removing_unknown_address()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new RemoveFavoriteAddress
            {
                AccountId = _accountId,
                AddressId = Guid.NewGuid()
            }));
        }

        [Test]
        public void when_address_updated_successfully()
        {
            this.sut.When(new UpdateFavoriteAddress { AccountId = _accountId, AddressId = _addressId, FriendlyName = "Chez Costo", FullAddress = "1234 rue Saint-Hubert" });

            Assert.AreEqual(1, sut.Events.Count());
            var evt = (FavoriteAddressUpdated)sut.Events.Single();
            Assert.AreEqual(_accountId, evt.SourceId);
            Assert.AreEqual(_addressId, evt.AddressId);
        }

        [Test]
        public void when_address_updated_with_missing_value()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new UpdateFavoriteAddress { AccountId = _accountId, AddressId = _addressId, FriendlyName = "Chez Costo"}));
        }
    }
}
