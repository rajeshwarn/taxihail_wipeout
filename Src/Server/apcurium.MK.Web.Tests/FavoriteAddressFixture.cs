using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;

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
            
            var otherAccount = sut.GetTestAccount(1);            

            sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            
            var adresses = sut.GetFavoriteAddresses(otherAccount.Id);

        }
       
    }
}
