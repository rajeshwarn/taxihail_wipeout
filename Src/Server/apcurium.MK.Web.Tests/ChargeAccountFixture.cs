using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class ChargeAccountFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            AsAdmin = true;
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        AdministrationServiceClient Sut { get; set; }

        private const string AccountNumber = "ROBF1";
        private const string CustomerNumber = "0";
        private readonly string[] PromptsToValidate = new[]
        {
            "1977-09-26",
            "M",
            "A"
        };

        private readonly KeyValuePair<string, string> accountToImport1 = new KeyValuePair<string, string>("ROBF", "0");
        private readonly KeyValuePair<string, string> accountToImport2 = new KeyValuePair<string, string>("3000", "0");
        private readonly KeyValuePair<string, string> accountToImport3 = new KeyValuePair<string, string>("3000", "1");
            

        private const bool PromptsAreValid = false;

        [Test]
        public void when_getting_all_accounts()
        {
            // There should be at least one account for the company
            var accounts = Sut.GetAllChargeAccount();
            Assert.IsNotEmpty(accounts);
        }

        [Test]
        public void when_getting_account_info()
        {
            // There should be an account number on the result
            var account = Sut.GetChargeAccount(AccountNumber, CustomerNumber);
            Assert.IsNotNullOrEmpty(account.AccountNumber);
        }

        [Test]
        public void when_validating_questions()
        {
            // Call should success
            var req = new IbsChargeAccountValidationRequest()
            {
                AccountNumber = AccountNumber,
                CustomerNumber = CustomerNumber,
                Prompts = PromptsToValidate
            };

            var validation = Sut.ValidateChargeAccount(req);
            Assert.AreEqual(validation.Message, "OK");
            Assert.AreEqual(validation.Valid, PromptsAreValid);
        }
    }
}