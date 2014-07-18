using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;

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
            _sut = new AdministrationServiceClient(BaseUrl, SessionId, new DummyPackageInfo());
        }

        [Test]
        public void AddAndGetAccountCharge()
        {
            var request = new AccountChargeRequest
            {
                Id = Guid.NewGuid(),
                Name = "VIP",
                Number = "NUMBER" + new Random(DateTime.Now.Millisecond).Next(0, 5236985),
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

            var account = _sut.GetAccountCharge(request.Number);

            Assert.That(account, Is.Not.Null);
        }

        [Test]
        public void Add_Already_Existing_AccountCharge()
        {
            var request = new AccountChargeRequest
            {
                Id = Guid.NewGuid(),
                Name = "VIP",
                Number = "NUMBER" + new Random(DateTime.Now.Millisecond).Next(0, 5236985),
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
        public void UpdatedAccountCharge()
        {
            var request = new AccountChargeRequest
            {
                Id = Guid.NewGuid(),
                Name = "VIP",
                Number = "NUMBER" + new Random(DateTime.Now.Millisecond).Next(0,5236985),
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

            var account = _sut.GetAccountCharge(request.Number);

            request.Id = account.Id;
            request.Name = "VIP2";

            _sut.UpdateAccountCharge(request);

            account = _sut.GetAccountCharge(request.Number);

            Assert.AreEqual(account.Name, request.Name);
        }

        [Test]
        public void DeleteAccountCharge()
        {
            var request = new AccountChargeRequest
            {
                Name = "VIP",
                Number = "NUMBER" + new Random(DateTime.Now.Millisecond).Next(0, 5236985),
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
            
            _sut.DeleteAccountCharge(request.Number);

            Assert.Throws<WebServiceException>(() =>  _sut.GetAccountCharge(request.Number));
        }
    }
}