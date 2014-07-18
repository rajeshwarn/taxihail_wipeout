using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Mobile.Infrastructure;
using NUnit.Framework;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class CompanyClientFixture : BaseTest
    {
        private CompanyServiceClient _sut;
        private DummyCache _cacheService;
        protected Func<DbContext> ContextFactory { get; set; }

        private CompanyDetail originalCompanyDetail;

        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();

            var connectionString = ConfigurationManager.ConnectionStrings["MKWebDev"].ConnectionString;
            ContextFactory = () => new BookingDbContext(connectionString);

            using (var context = ContextFactory.Invoke())
            {
                var companyDetail = context.Set<CompanyDetail>().First();
                originalCompanyDetail = new CompanyDetail
                {
                    Id = companyDetail.Id,
                    TermsAndConditions = companyDetail.TermsAndConditions,
                    Version = companyDetail.Version
                };
            }
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();

            using (var context = ContextFactory.Invoke())
            {
                var companyDetail = context.Set<CompanyDetail>().First();
                companyDetail.TermsAndConditions = originalCompanyDetail.TermsAndConditions;
                companyDetail.Version = originalCompanyDetail.Version;
                context.SaveChanges();
            }
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            CreateAndAuthenticateTestAdminAccount().Wait();
            _cacheService = new DummyCache();
            _sut = new CompanyServiceClient(BaseUrl, SessionId, new DummyPackageInfo(), _cacheService);
        }

        [Test]
        public async void FirstGet_ShouldGet_FullTerms_AndCache()
        {
            PrepareCompanyDetailWithoutTriggeredTerms();

            var terms = await _sut.GetTermsAndConditions();
            Assert.NotNull(terms);
            Assert.AreEqual(false, terms.Updated);
            Assert.NotNull(_cacheService.Get<object>("Terms"));
        }

        [Test]
        public async void FirstGet_ShouldGet_FullTerms_AndCache_if_terms_have_been_triggered_once()
        {
            PrepareCompanyDetailWithTriggeredTerms();

            var terms = await _sut.GetTermsAndConditions();
            Assert.NotNull(terms);
            Assert.AreEqual(true, terms.Updated);
            Assert.NotNull(_cacheService.Get<object>("Terms"));
        }

        [Test]
        public async void SecondGet_ShouldGet_Terms_NotUpdated()
        {
            PrepareCompanyDetailWithTriggeredTerms();

            var terms = await _sut.GetTermsAndConditions();

            Assert.NotNull(terms);
            Assert.AreEqual(true, terms.Updated);

            terms = await _sut.GetTermsAndConditions();
            Assert.NotNull(terms);
            Assert.AreEqual(false, terms.Updated);
        }

        [Test]
        public async void SecondGet_With_UpdateCompany_ShouldGet_Terms_Updated()
        {
            PrepareCompanyDetailWithTriggeredTerms();

            var terms = await _sut.GetTermsAndConditions();

            Assert.NotNull(terms);
            Assert.AreEqual(true, terms.Updated);

            TriggerTermsAndConditions();

            terms = await _sut.GetTermsAndConditions();

            Assert.NotNull(terms);
            Assert.AreEqual(true, terms.Updated);
        }

        private void PrepareCompanyDetailWithoutTriggeredTerms()
        {
            using (var context = ContextFactory.Invoke())
            {
                var companyDetail = context.Set<CompanyDetail>().Single(p => p.Id == AppConstants.CompanyId);
                companyDetail.TermsAndConditions = null;
                companyDetail.Version = null;
                context.SaveChanges();
            }
        }

        private void PrepareCompanyDetailWithTriggeredTerms()
        {
            using (var context = ContextFactory.Invoke())
            {
                var companyDetail = context.Set<CompanyDetail>().Single(p => p.Id == AppConstants.CompanyId);
                companyDetail.TermsAndConditions = "test";
                companyDetail.Version = "1";
                context.SaveChanges();
            }
        }

        private void TriggerTermsAndConditions()
        {
            using (var context = ContextFactory.Invoke())
            {
                var companyDetail = context.Set<CompanyDetail>().Single(p => p.Id == AppConstants.CompanyId);
                companyDetail.Version = "2";
                context.SaveChanges();
            }
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
                if (_cache.ContainsKey(key))
                {
                    _cache.Remove(key);
                }

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
