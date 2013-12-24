using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using apcurium.MK.Common.Caching;

namespace MK.Common.Tests.Caching
{
    [TestFixture]
    public class EFCacheClientFixture
    {
        protected string dbName;
        private EfCacheClient _sut;
        private string knownCacheKey;
        private object knownCacheObject;
        private string expiredCacheKey;


        [SetUp]
        public void Setup()
        {
            this._sut = new EfCacheClient(() => new CachingDbContext(dbName));
            knownCacheKey = Guid.NewGuid().ToString();
            knownCacheObject = Guid.NewGuid();
            expiredCacheKey = Guid.NewGuid().ToString();
            this._sut.Add(knownCacheKey, knownCacheObject);
            this._sut.Add(expiredCacheKey, Guid.NewGuid(), DateTime.Now.AddHours(-1));
        }

        [Test]
        public void when_cache_item_added()
        {
            var result = this._sut.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            Assert.IsTrue(result);
        }

        [Test]
        public void when_cache_item_added_with_same_key()
        {
            var result = this._sut.Add(knownCacheKey, Guid.NewGuid().ToString());

            Assert.IsFalse(result);
        }

        [Test]
        public void when_getting_item_from_cache()
        {
            var item = this._sut.Get<Guid>(knownCacheKey);

            Assert.AreEqual(knownCacheObject, item);
        }

        [Test]
        public void when_getting_expired_item_from_cache()
        {
            var item = this._sut.Get<Guid>(expiredCacheKey);
            Assert.AreEqual(Guid.Empty, item);
        }

        [Test]
        public void when_replacing_item_in_cache()
        {
            var expectedValue = Guid.NewGuid();
            this._sut.Set(knownCacheKey, expectedValue);

            var actualValue = this._sut.Get<Guid>(knownCacheKey);

            Assert.AreNotEqual(knownCacheObject, actualValue);
            Assert.AreEqual(expectedValue, actualValue);
        }



        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            dbName = this.GetType().Name + "-" + Guid.NewGuid().ToString();
            using (var context = new CachingDbContext(dbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            using (var context = new CachingDbContext(dbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();
            }
        }
    }
}
