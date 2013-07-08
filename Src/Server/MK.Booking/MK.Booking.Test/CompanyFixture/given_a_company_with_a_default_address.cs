using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_company_with_a_default_address
    {
        private EventSourcingTestHelper<Company> sut;
        private Guid _companyId = AppConstants.CompanyId;
        private Guid _addressId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            sut = new EventSourcingTestHelper<Company>();
            sut.Setup(new CompanyCommandHandler(sut.Repository));
            sut.Given(new CompanyCreated {SourceId = _companyId});
            sut.Given(new DefaultFavoriteAddressAdded
            {
                SourceId = _companyId,
                Address = new Address { Id = _addressId, FriendlyName = "default", Apartment = "3939", FullAddress = "1234 rue Saint-Hubert", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064 }
            });
        }

        [Test]
        public void when_default_address_removed_successfully()
        {
            sut.When(new RemoveDefaultFavoriteAddress { AddressId = _addressId });

            var evt = sut.ThenHasSingle<DefaultFavoriteAddressRemoved>();
            Assert.AreEqual(_addressId, evt.AddressId);
        }

        [Test]
        public void when_company_default_address_updated_successfully()
        {
            sut.When(new UpdateDefaultFavoriteAddress
            {
                Address = new Address { Id = _addressId, FriendlyName = "Chez Costo", FullAddress = "1234 rue Saint-Hubert", BuildingName = "Hôtel de Ville" }
            });

            var evt = sut.ThenHasSingle<DefaultFavoriteAddressUpdated>();
            Assert.AreEqual(_addressId, evt.Address.Id);
            Assert.AreEqual("Hôtel de Ville", evt.Address.BuildingName);
        }

        [Test]
        public void when_company_default_address_updated_with_missing_value()
        {
            Assert.Throws<InvalidOperationException>(() => sut.When(new UpdateDefaultFavoriteAddress
            {
                Address = new Address { Id = _addressId, FriendlyName = "Chez Costo" }
            }));
        }

    }
}
