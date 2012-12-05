﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.SelfHost;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class FavoriteAddressFixture: BaseTest
    {
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

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            var sut = new AccountServiceClient(BaseUrl, SessionId);
            sut.AddFavoriteAddress(new SaveAddress
            {
                Id = _knownAddressId,
                Address = new Address
                {
                    FriendlyName = "La Boite à Jojo",
                    FullAddress = "1234 rue Saint-Denis",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                }
            });
        }

        [Test]
        public void AddAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId);

            var addressId = Guid.NewGuid();
            sut.AddFavoriteAddress(new SaveAddress
                                       {
                                           Id = addressId,
                                           Address = new Address
                {
                    FriendlyName = "Chez François Cuvelier",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Hôtel de Ville",
                    Latitude = 45.515065,
                    Longitude = -73.558064
                }
                                       });

            var addresses = sut.GetFavoriteAddresses();

            Assert.AreEqual(1, addresses.Count(x => x.Id == addressId));
            var address = addresses.Single(x => x.Id == addressId);
            Assert.AreEqual("3939", address.Apartment);
            Assert.AreEqual("3131", address.RingCode);
            Assert.AreEqual("1234 rue Saint-Hubert", address.FullAddress);
            Assert.AreEqual("Hôtel de Ville", address.BuildingName);
            Assert.AreEqual(45.515065, address.Latitude);
            Assert.AreEqual(-73.558064, address.Longitude);
        }

        [Test]
        public void AddInvalidAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId);

            Assert.Throws<WebServiceException>(() => sut.AddFavoriteAddress(new SaveAddress()));
        }

        [Test]
        public void UpdateAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId);

            sut.UpdateFavoriteAddress(new SaveAddress
            {
                Id = _knownAddressId,
                Address = new Address
                {
                    FriendlyName = "Chez François Cuvelier",
                    Apartment = "3939",
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Le Manoir",
                    Latitude = 12,
                    Longitude = 34
                }
            });

            var address = sut.GetFavoriteAddresses().Single(x => x.Id == _knownAddressId);

            Assert.AreEqual("Chez François Cuvelier", address.FriendlyName);
            Assert.AreEqual("3939", address.Apartment);
            Assert.AreEqual("1234 rue Saint-Hubert", address.FullAddress);
            Assert.AreEqual("3131", address.RingCode);
            Assert.AreEqual("Le Manoir", address.BuildingName);
            Assert.AreEqual(12, address.Latitude);
            Assert.AreEqual(34, address.Longitude);

        }

        [Test]
        public void UpdateAddressWithInvalidData()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId);

            Assert.Throws<WebServiceException>(() => sut
                .UpdateFavoriteAddress(new SaveAddress
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

        }


        [Test]
        public void RemoveAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId);

            sut.RemoveFavoriteAddress(_knownAddressId);

            var addresses = sut.GetFavoriteAddresses();
            Assert.IsEmpty(addresses.Where(x => x.Id == _knownAddressId));
        }

        [Test]
        public void GetAddressList()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId);

            var addresses = sut.GetFavoriteAddresses();

            var knownAddress = addresses.SingleOrDefault(x => x.Id == _knownAddressId);
            Assert.IsNotNull(knownAddress);
        }
    }
}
