using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using MK.Common.Exceptions;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class CompanyAccountChargeFixture : BaseTest
    {
        private AdministrationServiceClient _sut;

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

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount().Wait();
            _sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), null);
        }

        [Test]
        public void AddAndGetAccountCharge()
        {
            var request = new AccountChargeRequest
            {
                Id = Guid.NewGuid(),
                Name = "1000",
                AccountNumber = "1000",
                Questions = new[]
                {
                    new AccountChargeQuestion
                    {
                        Question = "Question?",
                        Answer = "Answer"
                    }
                }
            };
            _sut.CreateAccountCharge(request);

            var account = _sut.GetAccountCharge(request.AccountNumber);

            Assert.That(account, Is.Not.Null);
        }

        [Test]
        public void Add_Already_Existing_AccountCharge()
        {
            var request = new AccountChargeRequest
            {
                Id = Guid.NewGuid(),
                Name = "VIP",
                AccountNumber = "NUMBER" + new Random(DateTime.Now.Millisecond).Next(0, 5236985),
                Questions = new[]
                {
                    new AccountChargeQuestion
                    {
                        Question = "Question?",
                        Answer = "Answer"
                    }
                }
            };
            _sut.CreateAccountCharge(request);

            Assert.Throws<WebServiceException>(() => _sut.CreateAccountCharge(request));
        }

        [Test]
        public void GetUnknownAccountCharge()
        {
            Assert.Throws<WebServiceException>(() => _sut.GetAccountCharge("UNKNOWN"));
        }

        [Test]
        public async Task UpdatedAccountCharge()
        {
            var request = new AccountChargeRequest
            {
                Id = Guid.NewGuid(),
                Name = "1000",
                AccountNumber = "1000",
                Questions = new[]
                {
                    new AccountChargeQuestion
                    {
                        Question = "Question?",
                        Answer = "Answer"
                    }
                }
            };

            var account = await _sut.GetAccountCharge(request.AccountNumber);
            if (account == null)
            {
                await _sut.CreateAccountCharge(request);
            }

            request.Id = account.Id;
            request.Name = "VIP2";

            await _sut.UpdateAccountCharge(request);

            account = await _sut.GetAccountCharge(request.AccountNumber);

            Assert.AreEqual(account.Name, request.Name);
        }

        [Test]
        public void DeleteAccountCharge()
        {
            var request = new AccountChargeRequest
            {
                Name = "VIP",
                AccountNumber = "NUMBER" + new Random(DateTime.Now.Millisecond).Next(0, 5236985),
                Questions = new[]
                {
                    new AccountChargeQuestion
                    {
                        Question = "Question?",
                        Answer = "Answer"
                    }
                }
            };
            _sut.CreateAccountCharge(request);
            
            _sut.DeleteAccountCharge(request.AccountNumber);

            Assert.Throws<WebServiceException>(() =>  _sut.GetAccountCharge(request.AccountNumber));
        }
    }
}