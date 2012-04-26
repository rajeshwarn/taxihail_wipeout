using System;
using NUnit.Framework;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services.Impl;

namespace TaxiMobile.Lib.Tests
{
    [TestFixture]
    public class AccountServiceFixture
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            ServiceLocator.Current.Register<IAppSettings, AppSettings>();
        }

        [Test]
        public void CreateAccountTest()
        {
            var account = new CreateAccountData
                              {
                                  Email = string.Format("apcurium@apcurium.com", Guid.NewGuid()),
                                  Password = "password",
                                  FirstName = "Matthieu",
                                  LastName = "Guyonnet-Duluc"
                              };

            var service = new AccountService();

            string error;
            var result = service.CreateAccount(account, out error);

            Assert.IsTrue(result);
        }
    }
}