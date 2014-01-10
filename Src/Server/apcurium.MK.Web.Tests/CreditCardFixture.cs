using System;
using System.Linq;
using System.Threading.Tasks;
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
        public async override Task Setup()
        {
            await base.Setup();
        }

        [TestFixtureSetUp]
        public async override Task TestFixtureSetup()
        {
            await base.TestFixtureSetup();
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
            const string friendlyName = "work credit card";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";
            const string token = "jjwcnSLWm85";

            await AccountService.AddCreditCard(new CreditCardRequest
            {
                CreditCardCompany = creditCardComapny,
                FriendlyName = friendlyName,
                CreditCardId = creditCardId,
                Last4Digits = last4Digits,
                Token = token
            });

            var creditCards = await AccountService.GetCreditCards();
            var creditcard = creditCards.First(x => x.CreditCardId == creditCardId);
            Assert.NotNull(creditcard);
            Assert.AreEqual(TestAccount.Id, creditcard.AccountId);
            Assert.AreEqual(creditCardComapny, creditcard.CreditCardCompany);
            Assert.AreEqual(friendlyName, creditcard.FriendlyName);
            Assert.AreEqual(creditCardId, creditcard.CreditCardId);
            Assert.AreEqual(last4Digits, creditcard.Last4Digits);
            Assert.AreEqual(token, creditcard.Token);
        }
        
        [Test]
        public async void RemoveCreditCard()
        {
            var client = GetFakePaymentClient();

            var sut = new AccountServiceClient(BaseUrl, SessionId, "Test", client);

            const string creditCardComapny = "visa";
            const string friendlyName = "work credit card";
            var creditCardId = Guid.NewGuid();
            const string last4Digits = "4025";

            var cc = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Cmt);
            var tokenResponse = await client.Tokenize(cc.Discover.Number, cc.Discover.ExpirationDate, cc.Discover.AvcCvvCvv2 + "");

            await sut.AddCreditCard(new CreditCardRequest
            {
                CreditCardCompany = creditCardComapny,
                FriendlyName = friendlyName,
                CreditCardId = creditCardId,
                Last4Digits = last4Digits,
                Token = tokenResponse.CardOnFileToken
            });

            await sut.RemoveCreditCard(creditCardId, tokenResponse.CardOnFileToken);

            var creditCards = await sut.GetCreditCards();
            Assert.IsEmpty(creditCards.Where(x => x.CreditCardId == creditCardId));
        }
    }
}