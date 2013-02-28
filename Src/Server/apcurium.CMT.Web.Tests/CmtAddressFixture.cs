using System;
using System.Linq;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;

namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    public class AddressCmtFixture : CmtBaseTest
    {

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
            Authenticate();
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
        }

        [Test]
        public void GetAddress()
        {
            var sut = new CmtAccountServiceClient(BaseUrl, Credentials);

            var addresses = sut.GetFavoriteAddresses();

            Assert.IsNotEmpty(addresses);
        }

        [Test]
        public void AddAddress()
        {
            var sut = new CmtAccountServiceClient(BaseUrl, Credentials);

            string friendlyName = ("ChezFrançois" + Guid.NewGuid()).Substring(0, 30);
            sut.AddFavoriteAddress(new SaveAddress
            {
                Address = new Address
                {
                    FriendlyName = friendlyName,
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Hôtel de Ville",
                    Latitude = 45.515065,
                    Longitude = -73.558064,
                    AddressType = "Home"
                }
            });

            var addresses = sut.GetFavoriteAddresses();

            
            Assert.AreEqual(1, addresses.Count(x => x.FriendlyName == friendlyName));
            var address = addresses.Single(x => x.FriendlyName == friendlyName);
            Assert.AreEqual("3131", address.RingCode);
            Assert.AreEqual("1234 rue Saint-Hubert", address.FullAddress);
            Assert.AreEqual("Hôtel de Ville", address.BuildingName);
            Assert.AreEqual(45.515065, address.Latitude);
            Assert.AreEqual(-73.558064, address.Longitude);
        }

        [Test]
        public void UpdateAddress()
        {
            var sut = new CmtAccountServiceClient(BaseUrl, Credentials);

            string friendlyName = ("ChezFrançois" + Guid.NewGuid()).Substring(0, 30);
            sut.AddFavoriteAddress(new SaveAddress
            {
                Address = new Address
                {
                    FriendlyName = friendlyName,
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Hôtel de Ville",
                    Latitude = 45.515065,
                    Longitude = -73.558064,
                    AddressType = "Home"
                }
            });


            var addresses = sut.GetFavoriteAddresses();
            var address = addresses.Single(x => x.FriendlyName == friendlyName);

            friendlyName = ("ChezFrançois" + Guid.NewGuid()).Substring(0, 30);
            sut.UpdateFavoriteAddress(new SaveAddress
            {
                Address = new Address
                {
                    Id = address.Id,
                    FriendlyName = friendlyName,
                    FullAddress = "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    BuildingName = "Hôtel de Ville",
                    Latitude = 45.515065,
                    Longitude = -73.558064,
                    AddressType = "Home"
                }
            });

            addresses = sut.GetFavoriteAddresses();

            Assert.AreEqual(friendlyName, addresses.Single(x => x.FriendlyName == friendlyName).FriendlyName);
        }

        [Test]
        public void RemoveAddress()
        {
            var sut = new CmtAccountServiceClient(BaseUrl, Credentials);

            string friendlyName = ("ChezFrançois" + Guid.NewGuid()).Substring(0, 30);
            sut.AddFavoriteAddress(new SaveAddress
                {
                    Address = new Address
                        {
                            FriendlyName = friendlyName,
                            FullAddress = "1234 rue Saint-Hubert",
                            RingCode = "3131",
                            BuildingName = "Hôtel de Ville",
                            Latitude = 45.515065,
                            Longitude = -73.558064,
                            AddressType = "Home"
                        }
                });

            var addresses = sut.GetFavoriteAddresses();


            var address = addresses.Single(x => x.FriendlyName == friendlyName);
            sut.RemoveAddress(address.Id);

            addresses = sut.GetFavoriteAddresses();

            Assert.AreEqual(0, addresses.Count(x => x.FriendlyName == friendlyName));
        }
    }
}