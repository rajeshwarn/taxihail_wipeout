using System;
using NUnit.Framework;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Services.Impl;

namespace TaxiMobile.Lib.Tests
{
    [TestFixture]
    public class AccountServiceFixture : BaseTest
    {
        private AccountService sut;

        [TestFixtureSetUp]
        public new void Setup()
        {
            base.Setup();
            sut = new AccountService();
        }

        [Test]
        public void CreateAccountTest()
        {
            var account = new CreateAccountData
                              {
                                  Email = string.Format("apcurium{0}@apcurium.com", Guid.NewGuid()),
                                  Password = "password",
                                  FirstName = "Matthieu",
                                  LastName = "Guyonnet-Duluc"
                              };


            string error;
            var result = sut.CreateAccount(account, out error);

            Assert.IsTrue(result);
        }

        [Test]
        public void GetAccountTest()
        {
            string error;
            sut.GetAccount("apcurium@apcurium.com", "password", out error);

            Assert.IsNullOrEmpty(error);
        }

        [Test]
        public void ResetPasswordTest()
        {
            string error;
            var account = new CreateAccountData
            {
                Email = "apcuriumtestrset@apcurium.com",
                Password = "password",
                FirstName = "Matthieu",
                LastName = "Guyonnet-Duluc"
            };

            //sut.CreateAccount(account, out error);
            
            var result =  sut.ResetPassword(new ResetPasswordData()
                                  {
                                      Email = account.Email,
                                      OldEmail = account.Email,
                                       OldPassword = "password",
                                      NewPassword = "password2"
                                  });

            Assert.IsTrue(result);

            sut.ResetPassword(new ResetPasswordData()
            {
                Email = account.Email,
                OldEmail = account.Email,
                OldPassword = "password2",
                NewPassword = "password"
            });

        }

        [Test]
        public void UpdateAccountTest()
        {
            //var account = new CreateAccountData
            //{
            //    Email = string.Format("apcuriumupdate@apcurium.com", Guid.NewGuid()),
            //    Password = "password",
            //    FirstName = "Matthieu",
            //    LastName = "Guyonnet-Duluc"
            //};
            //sut.CreateAccount(account, out error);

            string error;
           
            var existing = sut.GetAccount("apcuriumupdate@apcurium.com", "password", out error);
            existing.FirstName = "Matthieu" + new Random().Next(0, 100);

            sut.UpdateUser(existing);

            var updated = sut.GetAccount("apcuriumupdate@apcurium.com", "password", out error);
            Assert.AreEqual(existing.FirstName, updated.FirstName);
        }
    }
}