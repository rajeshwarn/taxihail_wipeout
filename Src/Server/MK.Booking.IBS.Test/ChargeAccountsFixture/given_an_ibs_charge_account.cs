#region

using System.Linq;
using System.Runtime;
using apcurium.MK.Booking.IBS.ChargeAccounts;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using NUnit.Framework;

#endregion

namespace MK.Booking.IBS.Test.ChargeAccountsFixture
{
    [TestFixture]
    public class given_an_ibs_charge_account
    {
        [SetUp]
        public void Setup()
        {
            Sut = new ChargeAccountWebServiceClient(new FakeServerSettings(), new FakeServerSettings().ServerData.IBS, new Logger());
        }

        ChargeAccountWebServiceClient Sut { get; set; }

        private const string AccountNumber = "ROBF1";
        private const string CustomerNumber = "0";

        [Test]
        public void when_getting_all_accounts()
        {
            var accounts = Sut.GetAllAccount();
            Assert.IsNotEmpty(accounts);
        }

        [Test]
        public void when_getting_account_info()
        {
            var account = Sut.GetIbsAccount(AccountNumber, CustomerNumber);
            Assert.IsNotNullOrEmpty(account.AccountNumber);
        }
    }
}