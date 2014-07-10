using System;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class FavoriteAddressFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            _knownAddressId = Guid.NewGuid();
            base.Setup();
            var sut = new AccountServiceClient(BaseUrl, SessionId, new PackageInfo());
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
                }).Wait();
        }

        private Guid _knownAddressId;

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
        public async void AddAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new PackageInfo());

            var addressId = Guid.NewGuid();
            await sut.AddFavoriteAddress(new SaveAddress
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

            var addresses = await sut.GetFavoriteAddresses();

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
            var sut = new AccountServiceClient(BaseUrl, SessionId, new PackageInfo());

            Assert.Throws<WebServiceException>(async () => await sut.AddFavoriteAddress(new SaveAddress()));
        }

        [Test]
        public async void GetAddressList()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new PackageInfo());

            var addresses = await sut.GetFavoriteAddresses();

            var knownAddress = addresses.SingleOrDefault(x => x.Id == _knownAddressId);
            Assert.IsNotNull(knownAddress);
        }

        [Test]
        public async void RemoveAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new PackageInfo());

            await sut.RemoveFavoriteAddress(_knownAddressId);

            var addresses = await sut.GetFavoriteAddresses();
            Assert.IsEmpty(addresses.Where(x => x.Id == _knownAddressId));
        }

        [Test]
        public async void UpdateAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, SessionId, new PackageInfo());

            await sut.UpdateFavoriteAddress(new SaveAddress
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

            var addresses = await sut.GetFavoriteAddresses();
            var address = addresses.Single(x => x.Id == _knownAddressId);

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
            var sut = new AccountServiceClient(BaseUrl, SessionId, new PackageInfo());

            Assert.Throws<WebServiceException>(async () => await sut.UpdateFavoriteAddress(new SaveAddress
                {
                    Id = _knownAddressId,
                    Address = new Address
                    {
                        FriendlyName = "Chez François Cuvelier",
                        Apartment = "3939",
                        FullAddress = "1234 rue Saint-Hubert",
                        RingCode = "3131",
                        Latitude = double.NaN,
                        Longitude = double.NaN
                    }
                }));
        }
    }
}