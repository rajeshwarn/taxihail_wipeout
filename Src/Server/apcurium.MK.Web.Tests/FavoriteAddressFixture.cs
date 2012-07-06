using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Web.SelfHost;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class FavoriteAddressFixture: BaseTest
    {
        private readonly Guid _knownAddressId = Guid.NewGuid();

        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();

            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            sut.AddFavoriteAddress(new SaveFavoriteAddress
            {
                Id = _knownAddressId,
                AccountId = TestAccount.Id,
                FriendlyName = "La Boite à Jojo",
                FullAddress = "1234 rue Saint-Denis",
                Latitude = 45.515065,
                Longitude = -73.558064
            });

        }

        [TestFixtureTearDown]
        public new void TearDown()
        {
            base.TearDown();
        } 

        [Test]
        public void AddAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            var addressId = Guid.NewGuid();
            sut.AddFavoriteAddress(new SaveFavoriteAddress
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
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            Assert.Throws<WebServiceException>(() => sut.AddFavoriteAddress(new SaveFavoriteAddress()));
        }

        [Test]
        public void UpdateAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            sut.UpdateFavoriteAddress(new SaveFavoriteAddress
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
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            Assert.Throws<WebServiceException>(() => sut
                .UpdateFavoriteAddress(new SaveFavoriteAddress
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
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            sut.RemoveFavoriteAddress(TestAccount.Id, _knownAddressId);

            var addresses = sut.GetFavoriteAddresses(TestAccount.Id);
            Assert.IsEmpty(addresses.Where(x => x.Id == _knownAddressId));
        }

        [Test]
        public void GetAddressList()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            var adresses = sut.GetFavoriteAddresses(TestAccount.Id);
                        
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Unauthorized")]
        public void GetAddressListFromDiffrentUser()
        {
            var sut = new AccountServiceClient(BaseUrl, null);

            var otherAccount = sut.GetTestAccount(1);            

            sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            
            var adresses = sut.GetFavoriteAddresses(otherAccount.Id);

        }
       
    }
}
