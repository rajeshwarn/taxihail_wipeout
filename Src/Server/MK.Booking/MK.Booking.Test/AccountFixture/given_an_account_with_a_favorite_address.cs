#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_an_account_with_a_favorite_address
    {
        [SetUp]
        public void SetUp()
        {
            _accountId = Guid.NewGuid();
            _addressId = Guid.NewGuid();
            _sut = new EventSourcingTestHelper<Account>();
            _sut.Setup(new AccountCommandHandler(_sut.Repository, null));
            _sut.Given(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob",
                Password = null,
                Email = "bob.smith@apcurium.com"
            });
            _sut.Given(new FavoriteAddressAdded
            {
                SourceId = _accountId,
                Address = new Address
                {
                    Id = _addressId,
                    FriendlyName = "Chez François",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                }
            });
        }

        private EventSourcingTestHelper<Account> _sut;
        private Guid _accountId;
        private Guid _addressId;

        [Test]
        public void when_address_removed_successfully()
        {
            _sut.When(new RemoveFavoriteAddress {AccountId = _accountId, AddressId = _addressId});

            var evt = _sut.ThenHasSingle<FavoriteAddressRemoved>();
            Assert.AreEqual(_accountId, evt.SourceId);
            Assert.AreEqual(_addressId, evt.AddressId);
        }

        [Test]
        public void when_address_updated_successfully()
        {
            _sut.When(new UpdateFavoriteAddress
            {
                AccountId = _accountId,
                Address =
                    new Address
                    {
                        Id = _addressId,
                        FriendlyName = "Chez Costo",
                        FullAddress = "1234 rue Saint-Hubert",
                        BuildingName = "Hôtel de Ville"
                    }
            });

            var evt = _sut.ThenHasSingle<FavoriteAddressUpdated>();
            Assert.AreEqual(_accountId, evt.SourceId);
            Assert.AreEqual(_addressId, evt.Address.Id);
            Assert.AreEqual("Hôtel de Ville", evt.Address.BuildingName);
            Assert.AreEqual("Chez Costo", evt.Address.FriendlyName);
            Assert.AreEqual("1234 rue Saint-Hubert", evt.Address.FullAddress);
        }

        [Test]
        public void when_address_updated_with_missing_value()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    _sut.When(new UpdateFavoriteAddress
                    {
                        AccountId = _accountId,
                        Address = new Address {Id = _addressId, FriendlyName = "Chez Costo"}
                    }));
        }

        [Test]
        public void when_removing_unknown_address()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.When(new RemoveFavoriteAddress
            {
                AccountId = _accountId,
                AddressId = Guid.NewGuid()
            }));
        }
    }
}