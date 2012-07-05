using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading;


namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class AccountFixture : BaseTest
    {

        private const string _testUserEmail = "apcurium.test@apcurium.com";
        private const string _testUserPassword = "password";


        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();
            //sut = new AccountService();
        }

        [Test]
        public void BasicSignIn()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, TestAccountPassword));
            var acc = sut.GetMyAccount();
            
            Assert.IsNotNull(acc);
            Assert.AreEqual(acc.Id, TestAccount.Id);
            Assert.AreEqual(acc.Email, TestAccount.Email);
            Assert.AreEqual(acc.FirstName, TestAccount.FirstName);
            Assert.AreEqual(acc.LastName, TestAccount.LastName);
            Assert.AreEqual(acc.Phone, TestAccount.Phone);
            
        }

        [Test]
        [ExpectedException( "ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage="Unauthorized")]
        public void BasicSignInWithInvalidPassword()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo(TestAccount.Email, "wrong_password"));
            var acc = sut.GetMyAccount();            
        }

        [Test]
        [ExpectedException("ServiceStack.ServiceClient.Web.WebServiceException", ExpectedMessage = "Unauthorized")]
        public void BasicSignInWithInvalidEmail()
        {
            var sut = new AccountServiceClient(BaseUrl, new AuthInfo("wrong_email@wrong.com", "password1"));
            var acc = sut.GetMyAccount();
        }


        [Test]
        public void RegisteringAccountTest()
        {
            var sut = new AccountServiceClient(BaseUrl, null);
            var newAccount = new RegisterAccount { AccountId = Guid.NewGuid(), Phone =  "5146543024", Email = GetTempEmail(), FirstName = "First Name Test", LastName = "Last Name Test", Password = "password" };
            sut.RegisterAccount(newAccount);
            Thread.Sleep(1000);
            sut = new AccountServiceClient(BaseUrl, new AuthInfo(newAccount.Email, newAccount.Password));
            var account = sut.GetMyAccount();
            Assert.IsNotNull(account);
            Assert.AreEqual(newAccount.AccountId, account.Id);
        }
       
        


        private string GetTempEmail()
        {
            var email = string.Format("testemail.{0}@apcurium.com", Guid.NewGuid().ToString().Replace("-", ""));
            return email;
        }



        //[Test]
        //public void GetAccountTest()
        //{
        //    string error;
        //    sut.GetAccount("apcurium@apcurium.com", "password", out error);

        //    Assert.IsNullOrEmpty(error);
        //}

        //[Test]
        //public void ResetPasswordTest()
        //{
        //    string error;
        //    var account = new CreateAccountData
        //    {
        //        Email = "apcuriumtestrset@apcurium.com",
        //        Password = "password",
        //        FirstName = "Matthieu",
        //        LastName = "Guyonnet-Duluc"
        //    };

        //    //sut.CreateAccount(account, out error);

        //    var result = sut.ResetPassword(new ResetPasswordData()
        //    {
        //        Email = account.Email,
        //        OldEmail = account.Email,
        //        OldPassword = "password",
        //        NewPassword = "password2"
        //    });

        //    Assert.IsTrue(result);

        //    sut.ResetPassword(new ResetPasswordData()
        //    {
        //        Email = account.Email,
        //        OldEmail = account.Email,
        //        OldPassword = "password2",
        //        NewPassword = "password"
        //    });

        //}

        //[Test]
        //public void UpdateAccountTest()
        //{
        //    //var account = new CreateAccountData
        //    //{
        //    //    Email = string.Format("apcuriumupdate@apcurium.com", Guid.NewGuid()),
        //    //    Password = "password",
        //    //    FirstName = "Matthieu",
        //    //    LastName = "Guyonnet-Duluc"
        //    //};
        //    //sut.CreateAccount(account, out error);

        //    string error;

        //    var existing = sut.GetAccount("apcuriumupdate@apcurium.com", "password", out error);
        //    existing.FirstName = "Matthieu" + new Random().Next(0, 100);

        //    sut.UpdateUser(existing);

        //    var updated = sut.GetAccount("apcuriumupdate@apcurium.com", "password", out error);
        //    Assert.AreEqual(existing.FirstName, updated.FirstName);
        //}

    }
}
