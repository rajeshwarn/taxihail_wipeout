﻿using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Diagnostic;
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
            Sut = new IbsChargeAccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
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

        IbsChargeAccountServiceClient Sut { get; set; }

        private const string AccountNumber = "ROBF1";
        private const string CustomerNumber = "0";
        private readonly string[] PromptsToValidate = new[]
        {
            "1977-09-26",
            "M",
            "A"
        };

        private const bool PromptsAreValid = false;

        [Test]
        public async void when_getting_all_accounts()
        {
            // There should be at least one account for the company
            var accounts = await Sut.GetAllChargeAccount();
            Assert.IsNotEmpty(accounts);
        }

        [Test]
        public async void when_getting_account_info()
        {
            // There should be an account number on the result
            var account = await Sut.GetChargeAccount(AccountNumber, CustomerNumber);
            Assert.IsNotNullOrEmpty(account.AccountNumber);
        }

        [Test]
        public async void when_validating_questions()
        {
            // Call should success
            var req = new IbsChargeAccountValidationRequest()
            {
                AccountNumber = AccountNumber,
                CustomerNumber = CustomerNumber,
                Prompts = PromptsToValidate
            };

            var validation = await Sut.ValidateChargeAccount(req);
            Assert.AreEqual(validation.Message, "OK");
            Assert.AreEqual(validation.Valid, PromptsAreValid);
        }
    }
}