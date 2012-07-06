using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Web.SelfHost;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class FavoriteAddressFixture: BaseTest
    {

        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();

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

            var acc = sut.GetMyAccount();
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
        public void UpdateAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            var acc = sut.GetMyAccount();

            sut.UpdateFavoriteAddress(new SaveFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AccountId = TestAccount.Id,
                FriendlyName = "Chez François Cuvelier",
                Apartment = "3939",
                FullAddress = "1234 rue Saint-Hubert",
                RingCode = "3131",
                Latitude = 45.515065,
                Longitude = -73.558064
            });

        }

        [Test]
        public void RemoveAddress()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            var acc = sut.GetMyAccount();

            sut.RemoveFavoriteAddress(acc.Id, Guid.NewGuid());
        }

        [Test]
        public void GetAddressList()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));

            var acc = sut.GetMyAccount(); 
            
            var adresses = sut.GetFavoriteAddresses(acc.Id);
                        
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Unauthorized")]
        public void GetAddressListFromDiffrentUser()
        {
            var sut = new AccountServiceClient(BaseUrl, null);
            
            var otherAccount = sut.GetTestAccount(0);            

            sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            
            var adresses = sut.GetFavoriteAddresses(otherAccount.Id);

        }
       
    }
}
