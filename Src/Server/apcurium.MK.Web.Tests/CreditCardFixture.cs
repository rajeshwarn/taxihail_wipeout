using System;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class CreditCardFixture : BaseTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [Test]
        public async void AddCreditCard()
        {
            const string creditCardComapny = "visa";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            await AccountService.AddCreditCard(new CreditCardRequest
            {
                CreditCardCompany = creditCardComapny,
                CreditCardId = creditCardId,
                Last4Digits = last4Digits,
                Token = token
            });

            var creditCards = await AccountService.GetCreditCards();
            var creditcard = creditCards.First(x => x.CreditCardId == creditCardId);
            Assert.NotNull(creditcard);
            Assert.AreEqual(TestAccount.Id, creditcard.AccountId);
            Assert.AreEqual(creditCardComapny, creditcard.CreditCardCompany);
            Assert.AreEqual(creditCardId, creditcard.CreditCardId);
            Assert.AreEqual(last4Digits, creditcard.Last4Digits);
            Assert.AreEqual(token, creditcard.Token);
        }
        
        [Test]
        public async void RemoveCreditCard()
        {
            var client = GetFakePaymentClient();

            var sut = new AccountServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), client);

            const string creditCardComapny = "visa";
            const string nameOnCard = "Bob";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string expirationMonth = "5";
            const string expirationYear = "2020";

            var cc = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Cmt);
            var tokenResponse = await client.Tokenize(cc.Discover.Number, cc.Discover.ExpirationDate, cc.Discover.AvcCvvCvv2 + "");

            await sut.AddCreditCard(new CreditCardRequest
            {
                CreditCardCompany = creditCardComapny,
                NameOnCard = nameOnCard,
                CreditCardId = creditCardId,
                Last4Digits = last4Digits,
                ExpirationMonth = expirationMonth,
                ExpirationYear = expirationYear,
                Token = tokenResponse.CardOnFileToken
            });

            await sut.RemoveCreditCard();

            var creditCards = await sut.GetCreditCards();
            Assert.IsEmpty(creditCards.Where(x => x.CreditCardId == creditCardId));
        }
    }
}