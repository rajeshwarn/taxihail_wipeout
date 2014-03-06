using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class CompanyClientFixture : BaseTest
    {
        private CompanyServiceClient _sut;
        private DummyCache _cacheService;

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
            _cacheService = new DummyCache();
            _sut = new CompanyServiceClient(BaseUrl, SessionId, "Test", _cacheService);
        }

        [Test]
        public async void FirstGet_ShouldGet_FullTerms_AndCache()
        {
            var terms = await _sut.GetTermsAndConditions();
            Assert.NotNull(terms);
            Assert.AreEqual(true, terms.Updated);
            Assert.NotNull(_cacheService.Get<object>("Terms"));
        }

        [Test]
        [Ignore("this needs the API to update the terms and condition")]
        public async void SecondGet_ShouldGet_Terms_NotUpdated()
        {
            var terms = await _sut.GetTermsAndConditions();
            terms = await _sut.GetTermsAndConditions();
            Assert.NotNull(terms);
            Assert.AreEqual(false, terms.Updated);
        }

        [Test]
        [Ignore("this needs the API to update the terms and condition")]
        public async void SecondGet_With_UpdateCompany_ShouldGet_Terms_NotUpdated()
        {
            var terms = await _sut.GetTermsAndConditions();

            //update terms on server

            //second get
            terms = await _sut.GetTermsAndConditions();

            //assert
            Assert.NotNull(terms);
            Assert.AreEqual(true, terms.Updated);
        }

        public class DummyCache : ICacheService
        {
            Dictionary<string, object> _cache = new Dictionary<string, object>();
            public T Get<T>(string key) where T : class
            {
                return _cache.ContainsKey(key) ? (T)_cache[key] : default(T);
            }

            public void Set<T>(string key, T obj) where T : class
            {
                _cache.Add(key, obj);
            }

            public void Set<T>(string key, T obj, DateTime expiresAt) where T : class
            {
               
            }

            public void Clear(string key)
            { 
            }

            public void ClearAll()
            {
               
            }
        }
    }
}
