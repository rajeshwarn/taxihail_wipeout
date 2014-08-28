#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.CompanyFixture
{
    [TestFixture]
    public class given_a_company_with_a_default_address
    {
        [SetUp]
        public void SetUp()
        {
            _sut = new EventSourcingTestHelper<Company>();
            _sut.Setup(new CompanyCommandHandler(_sut.Repository, null));
            _sut.Given(new CompanyCreated {SourceId = _companyId});
            _sut.Given(new DefaultFavoriteAddressAdded
            {
                SourceId = _companyId,
                Address =
                    new Address
                    {
                        Id = _addressId,
                        FriendlyName = "default",
                        Apartment = "3939",
                        FullAddress = "1234 rue Saint-Hubert",
                        RingCode = "3131",
                        Latitude = 45.515065,
                        Longitude = -73.558064
                    }
            });
        }

        private EventSourcingTestHelper<Company> _sut;
        private readonly Guid _companyId = AppConstants.CompanyId;
        private readonly Guid _addressId = Guid.NewGuid();

        [Test]
        public void when_company_default_address_updated_successfully()
        {
            _sut.When(new UpdateDefaultFavoriteAddress
            {
                Address =
                    new Address
                    {
                        Id = _addressId,
                        FriendlyName = "Chez Costo",
                        FullAddress = "1234 rue Saint-Hubert",
                        BuildingName = "Hôtel de Ville"
                    }
            });

            var evt = _sut.ThenHasSingle<DefaultFavoriteAddressUpdated>();
            Assert.AreEqual(_addressId, evt.Address.Id);
            Assert.AreEqual("Hôtel de Ville", evt.Address.BuildingName);
        }

        [Test]
        public void when_company_default_address_updated_with_missing_value()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.When(new UpdateDefaultFavoriteAddress
            {
                Address = new Address {Id = _addressId, FriendlyName = "Chez Costo"}
            }));
        }

        [Test]
        public void when_default_address_removed_successfully()
        {
            _sut.When(new RemoveDefaultFavoriteAddress {AddressId = _addressId});

            var evt = _sut.ThenHasSingle<DefaultFavoriteAddressRemoved>();
            Assert.AreEqual(_addressId, evt.AddressId);
        }
    }
}