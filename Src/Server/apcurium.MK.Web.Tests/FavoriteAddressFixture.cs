using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
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
            var sut = new AccountServiceClient(BaseUrl);
            sut.AddFavoriteAddress(new SaveAddress
            {
                Id = (_knownAddressId = Guid.NewGuid()),
                AccountId = TestAccount.Id,
                FriendlyName = "La Boite à Jojo",
                FullAddress = "1234 rue Saint-Denis",
                Latitude = 45.515065,
                Longitude = -73.558064
            });
        }

        [Test]
        public void AddAddress()
        {
            var sut = new AccountServiceClient(BaseUrl);

            var addressId = Guid.NewGuid();
            sut.AddFavoriteAddress(new SaveAddress
                                       {
                                           Id = addressId,
                                           AccountId = TestAccount.Id,
                                           FriendlyName = "Chez François Cuvelier",
                                           Apartment = "3939",
                                           FullAddress = "1234 rue Saint-Hubert",
                                           RingCode = "3131",
                                           Latitude = 45.515065,
                                           Longitude = -73.558064
                                       });

            var addresses = sut.GetFavoriteAddresses(TestAccount.Id);

            Assert.AreEqual(1, addresses.Count(x => x.Id == addressId));
        }

        [Test]
        public void AddInvalidAddress()
        {
            var sut = new AccountServiceClient(BaseUrl);

            Assert.Throws<WebServiceException>(() => sut.AddFavoriteAddress(new SaveAddress()));
        }

        [Test]
        public void UpdateAddress()
        {
            var sut = new AccountServiceClient(BaseUrl);

            sut.UpdateFavoriteAddress(new SaveAddress
            {
                Id = _knownAddressId,
                AccountId = TestAccount.Id,
                FriendlyName = "Chez François Cuvelier",
                Apartment = "3939",
                FullAddress = "1234 rue Saint-Hubert",
                RingCode = "3131",
                Latitude = 12,
                Longitude = 34
            });

            var address = sut.GetFavoriteAddresses(TestAccount.Id).Single(x => x.Id == _knownAddressId);

            Assert.AreEqual("Chez François Cuvelier", address.FriendlyName);
            Assert.AreEqual("3939", address.Apartment);
            Assert.AreEqual("1234 rue Saint-Hubert", address.FullAddress);
            Assert.AreEqual("3131", address.RingCode);
            Assert.AreEqual(12, address.Latitude);
            Assert.AreEqual(34, address.Longitude);

        }

        [Test]
        public void UpdateAddressWithInvalidData()
        {
            var sut = new AccountServiceClient(BaseUrl);

            Assert.Throws<WebServiceException>(() => sut
                .UpdateFavoriteAddress(new SaveAddress
                {
                    Id = _knownAddressId,
                    AccountId = TestAccount.Id,
                    FriendlyName =
                        "Chez François Cuvelier",
                    Apartment = "3939",
                    FullAddress =
                        "1234 rue Saint-Hubert",
                    RingCode = "3131",
                    Latitude = double.NaN,
                    Longitude = double.NaN
                }));

        }


        [Test]
        public void RemoveAddress()
        {
            var sut = new AccountServiceClient(BaseUrl);

            sut.RemoveFavoriteAddress(TestAccount.Id, _knownAddressId);

            var addresses = sut.GetFavoriteAddresses(TestAccount.Id);
            Assert.IsEmpty(addresses.Where(x => x.Id == _knownAddressId));
        }

        [Test]
        public void GetAddressList()
        {
            var sut = new AccountServiceClient(BaseUrl);

            var addresses = sut.GetFavoriteAddresses(TestAccount.Id);

            var knownAddress = addresses.SingleOrDefault(x => x.Id == _knownAddressId);
            Assert.IsNotNull(knownAddress);
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Unauthorized")]
        public void GetAddressListFromDiffrentUser()
        {
            var sut = new AccountServiceClient(BaseUrl);
            var otherAccount = sut.GetTestAccount(1);            
            
            var adresses = sut.GetFavoriteAddresses(otherAccount.Id);

        }

        [Test]
        public void when_save_a_favorite_address_with_an_historic_address_existing()
        {
            //Setup
            var newAccount = GetNewAccount();

            var orderService = new OrderServiceClient(BaseUrl);

            var order = new CreateOrder
            {
                Id = Guid.NewGuid(),
                AccountId = newAccount.Id,
                PickupDate = DateTime.Now,
                PickupAddress = new Booking.Api.Contract.Resources.Address { FullAddress = "1234 rue Saint-Denis", Apartment = "3939", RingCode = "3131", Latitude = 45.515065, Longitude = -73.558064, },
                DropOffAddress = new Booking.Api.Contract.Resources.Address { FullAddress = "Velvet auberge st gabriel", Latitude = 45.50643, Longitude = -73.554052 },
                Settings = new Booking.Api.Contract.Resources.BookingSettings { ChargeTypeId = 99, VehicleTypeId = 88, ProviderId = 11, Phone = "514-555-1212", Passengers = 6, NumberOfTaxi = 1, Name = "Joe Smith" }

            };
            orderService.CreateOrder(order);

            //Arrange
            var sut = new AccountServiceClient(BaseUrl);

            //Act
            Guid addressGuid = Guid.NewGuid();
            var address = new SaveAddress()
            {
                Id = addressGuid,
                AccountId = newAccount.Id,
                FriendlyName = "La Boite à Jojo",
                FullAddress = "1234 rue Saint-Denis",
                Latitude = 45.515065,
                Longitude = -73.558064,
                Apartment = "3939",
                RingCode = "3131"
            };
            sut.AddFavoriteAddress(address);

            //Assert
            var addresses = sut.GetHistoryAddresses(newAccount.Id);

            Address first = addresses.FirstOrDefault(address1 => address1.Id.Equals(addressGuid));
            Assert.IsNull(first);

        }
       
    }
}
