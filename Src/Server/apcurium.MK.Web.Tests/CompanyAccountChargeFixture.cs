﻿using System;
using System.Net;
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
        public async Task Add_Already_Existing_AccountCharge()
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
            await _sut.CreateAccountCharge(request);

            try
            {
                await _sut.CreateAccountCharge(request);
            }
            catch (Exception ex)
            {
                var exception = Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });

                Assert.AreEqual((int)HttpStatusCode.Conflict, exception.StatusCode);

                return;
            }

            Assert.Fail();
        }

        [Test]
        public async Task GetUnknownAccountCharge()
        {
            try
            {
                await _sut.GetAccountCharge("UNKNOWN");
            }
            catch (Exception ex)
            {
                var exception = Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });

                Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);

                return;
            }

            Assert.Fail();
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

            Booking.Api.Contract.Resources.AccountCharge account = null;

            try
            {
                account = await _sut.GetAccountCharge(request.AccountNumber);;
            }
            catch
            {
                // We are suppressing this error. 
            }

            if (account == null)
            {
                await _sut.CreateAccountCharge(request);

                account = await _sut.GetAccountCharge(request.AccountNumber);
            }

            request.Id = account.Id;
            request.Name = "VIP2";

            await _sut.UpdateAccountCharge(request);

            account = await _sut.GetAccountCharge(request.AccountNumber);

            Assert.AreEqual(account.Name, request.Name);
        }

        [Test]
        public async Task DeleteAccountCharge()
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
            await _sut.CreateAccountCharge(request);
            
            await _sut.DeleteAccountCharge(request.AccountNumber);

            try
            {
                await _sut.GetAccountCharge(request.AccountNumber);
            }
            catch (Exception ex)
            {
                var exception = Assert.Throws<WebServiceException>(() =>
                {
                    throw ex;
                });

                Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);

                return;
            }

            Assert.Fail();
        }
    }
}