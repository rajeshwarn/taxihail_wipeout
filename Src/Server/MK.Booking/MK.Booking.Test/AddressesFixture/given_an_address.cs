using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Test.AddressesFixture
{
    [TestFixture]
    public class given_an_address
    {
        private EventSourcingTestHelper<Account> sut;
        private EventSourcingTestHelper<Company> companySut;
        private readonly Guid _accountId = Guid.NewGuid();
        private readonly Guid _addressId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Account>();
            this.companySut = new EventSourcingTestHelper<Company>();
            this.companySut.Setup(new AddressCommandHandler(this.sut.Repository, this.companySut.Repository));
            this.sut.Setup(new AddressCommandHandler(this.sut.Repository, this.companySut.Repository));
            this.sut.Given(new AccountRegistered { SourceId = _accountId, Name = "Bob", Password = null, Email = "bob.smith@apcurium.com" });
            this.companySut.Given(new CompanyCreated { SourceId = AppConstants.CompanyId});
            this.sut.Given(new FavoriteAddressAdded { AddressId = _addressId, SourceId = _accountId, FriendlyName = "Chez François", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 });
            this.sut.Given(new DefaultFavoriteAddressAdded { AddressId = _addressId, FriendlyName = "Chez François", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 });
            this.sut.Given(new PopularAddressAdded { AddressId = _addressId, FriendlyName = "Chez François popular", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 });
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
        public void when_company_default_address_removed_successfully()
        {
            this.companySut.When(new RemoveDefaultFavoriteAddress { AddressId = _addressId });

            Assert.AreEqual(1, companySut.Events.Count);
            var evt = (DefaultFavoriteAddressRemoved)companySut.Events[0];
            Assert.AreEqual(_addressId, evt.AddressId);
        }

        [Test]
        public void when_company_popular_address_removed_successfully()
        {
            this.companySut.When(new RemovePopularAddress { AddressId = _addressId });

            Assert.AreEqual(1, companySut.Events.Count);
            var evt = (PopularAddressRemoved)companySut.Events[0];
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
            this.sut.When(new UpdateFavoriteAddress { AccountId = _accountId, AddressId = _addressId, FriendlyName = "Chez Costo", FullAddress = "1234 rue Saint-Hubert", BuildingName = "Hôtel de Ville" });

            Assert.AreEqual(1, sut.Events.Count());
            var evt = (FavoriteAddressUpdated)sut.Events.Single();
            Assert.AreEqual(_accountId, evt.SourceId);
            Assert.AreEqual(_addressId, evt.AddressId);
            Assert.AreEqual("Hôtel de Ville", evt.BuildingName);
        }

        [Test]
        public void when_company_default_address_updated_successfully()
        {
            this.companySut.When(new UpdateDefaultFavoriteAddress { AddressId = _addressId, FriendlyName = "Chez Costo", FullAddress = "1234 rue Saint-Hubert", BuildingName = "Hôtel de Ville" });

            Assert.AreEqual(1, companySut.Events.Count());
            var evt = (DefaultFavoriteAddressUpdated)companySut.Events[0];
            Assert.AreEqual(_addressId, evt.AddressId);
            Assert.AreEqual("Hôtel de Ville", evt.BuildingName);
        }

        [Test]
        public void when_company_popular_address_updated_successfully()
        {
            this.companySut.When(new UpdatePopularAddress { AddressId = _addressId, FriendlyName = "Chez Costo popular", FullAddress = "1234 rue Saint-Hubert", BuildingName = "Hôtel de Ville" });

            Assert.AreEqual(1, companySut.Events.Count());
            var evt = (PopularAddressUpdated)companySut.Events[0];
            Assert.AreEqual(_addressId, evt.AddressId);
            Assert.AreEqual("Hôtel de Ville", evt.BuildingName);
        }

        [Test]
        public void when_address_updated_with_missing_value()
        {
            Assert.Throws<InvalidOperationException>(() => this.sut.When(new UpdateFavoriteAddress { AccountId = _accountId, AddressId = _addressId, FriendlyName = "Chez Costo"}));
        }

        [Test]
        public void when_company_default_address_updated_with_missing_value()
        {
            Assert.Throws<InvalidOperationException>(() => this.companySut.When(new UpdateDefaultFavoriteAddress { AddressId = _addressId, FriendlyName = "Chez Costo" }));
        }

        [Test]
        public void when_company_popular_address_updated_with_missing_value()
        {
            Assert.Throws<InvalidOperationException>(() => this.companySut.When(new UpdatePopularAddress { AddressId = _addressId, FriendlyName = "Chez Costo" }));
        }

        [Test]
        public void when_removing_address_successfully()
        {
            var _addressId = Guid.NewGuid();
            this.sut.When(new RemoveAddressFromHistory { AddressId = _addressId, AccountId = _accountId });

            var @event = sut.ThenHasSingle<AddressRemovedFromHistory>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual(_addressId, @event.AddressId);
        }
    }
}
