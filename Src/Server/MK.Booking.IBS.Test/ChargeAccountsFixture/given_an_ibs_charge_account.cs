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
            Sut = new ChargeAccountWebServiceClient(new FakeServerSettings(), new FakeServerSettings().ServerData.IBS,
                new Logger());
        }

        ChargeAccountWebServiceClient Sut { get; set; }

        private const string AccountNumber = "ROBF1";
        private const string CustomerNumber = "0";
        private readonly string[] PromptsToValidate = new[]
        {
            "1977-09-26",
            "M",
            "A"
        };

        [Test]
        public void when_getting_all_accounts()
        {
            // There should be at least one account for the company
            var accounts = Sut.GetAllAccount();
            Assert.IsNotEmpty(accounts);
        }

        [Test]
        public void when_getting_account_info()
        {
            // There should be an account number on the result
            var account = Sut.GetIbsAccount(AccountNumber, CustomerNumber);
            Assert.IsNotNullOrEmpty(account.AccountNumber);
        }

        [Test, Ignore("Was broken by MK")]
        public void when_getting_account_info_with_invalid_customer_number()
        {
            // There should be an account number on the result
            var account = Sut.GetIbsAccount(AccountNumber, "654564");
            Assert.AreEqual("account not found", account.Message);
        }

        [Test]
        public void when_validating_questions()
        {
            // Call should success
            var validation = Sut.ValidateIbsChargeAccount(PromptsToValidate, AccountNumber, CustomerNumber);
            Assert.AreEqual(validation.Message, "OK");
        }
    }
}