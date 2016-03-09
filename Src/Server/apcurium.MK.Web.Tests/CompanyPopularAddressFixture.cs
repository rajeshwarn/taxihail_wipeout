﻿using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using MK.Common.Exceptions;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class CompanyPopularAddressFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount().Wait();
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
            sut.AddPopularAddress(new PopularAddress
            {
                Id = (_knownAddressId = Guid.NewGuid()),
                Address = new Address
                {
                    FriendlyName = "La Boite à Jojo le barjo popular",
                    FullAddress = "1234 rue Saint-Denis",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                }
            });
        }

        private Guid _knownAddressId = Guid.NewGuid();

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [Test]
        public async Task AddAddress()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);

            var addressId = Guid.NewGuid();
            await sut.AddPopularAddress(new PopularAddress
            {
                Id = addressId,
                Address = new Address
                {
                    FriendlyName = "Chez François Cuvelier le bg popular",
                    Apartment = "39398",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Hôtel de Ville",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                }
            });

            var addresses = await sut.GetPopularAddresses();

            Assert.AreEqual(1, addresses.Count(x => x.Id == addressId));
            var address = addresses.Single(x => x.Id == addressId);
            Assert.AreEqual("39398", address.Apartment);
            Assert.AreEqual("3131", address.RingCode);
            Assert.AreEqual("1234 rue Saint-Hubert", address.FullAddress);
            Assert.AreEqual("Hôtel de Ville", address.BuildingName);
            Assert.AreEqual(45.515065, address.Latitude);
            Assert.AreEqual(-73.558064, address.Longitude);
        }

        [Test]
        public void AddInvalidAddress()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);

            Assert.Throws<WebServiceException>(() => sut.AddPopularAddress(new PopularAddress()));
        }

        [Test]
        public async Task GetAddressList()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);

            var addresses = await sut.GetPopularAddresses();

            var knownAddress = addresses.SingleOrDefault(x => x.Id == _knownAddressId);
            Assert.IsNotNull(knownAddress);
        }

        [Test]
        public async Task RemoveAddress()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);

            await sut.RemovePopularAddress(_knownAddressId);

            var addresses = await sut.GetPopularAddresses();
            Assert.IsEmpty(addresses.Where(x => x.Id == _knownAddressId));
        }

        [Test]
        public async Task UpdateAddress()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);

            await sut.UpdatePopularAddress(new PopularAddress
            {
                Id = _knownAddressId,
                Address = new Address
                {
                    FriendlyName = "Chez François Cuvelier popular",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Le Manoir playboy",
                    Latitude = 12,
                    Longitude = 34
                }
            });

            var address = (await sut.GetPopularAddresses()).Single(x => x.Id == _knownAddressId);

            Assert.AreEqual("Chez François Cuvelier popular", address.FriendlyName);
            Assert.AreEqual("3939", address.Apartment);
            Assert.AreEqual("1234 rue Saint-Hubert", address.FullAddress);
            Assert.AreEqual("3131", address.RingCode);
            Assert.AreEqual("Le Manoir playboy", address.BuildingName);
            Assert.AreEqual(12, address.Latitude);
            Assert.AreEqual(34, address.Longitude);
        }

        [Test]
        public void UpdateAddressWithInvalidData()
        {
            var sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);

            var ex = Assert.Throws<WebServiceException>(() => sut.UpdatePopularAddress(new PopularAddress
                {
                    Id = _knownAddressId,
                    Address = new Address
                    {
                        FriendlyName =
                            "Chez François Cuvelier",
                        Apartment = "3939",
                        FullAddress =
                            "1234 rue Saint-Hubert",
                        RingCode = "3131",
                        Latitude = double.NaN,
                        Longitude = double.NaN
                    }
                }));

            Assert.AreEqual("InclusiveBetween", ex.Message);
        }
    }
}