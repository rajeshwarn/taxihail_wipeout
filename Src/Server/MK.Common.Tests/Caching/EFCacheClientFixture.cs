#region

using System;
using apcurium.MK.Common.Caching;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Common.Tests.Caching
{
    [TestFixture]
    public class EfCacheClientFixture
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EfCacheClient(() => new CachingDbContext(DbName));
            _knownCacheKey = Guid.NewGuid().ToString();
            _knownCacheObject = Guid.NewGuid();
            _expiredCacheKey = Guid.NewGuid().ToString();
            _sut.Add(_knownCacheKey, _knownCacheObject);
            _sut.Add(_expiredCacheKey, Guid.NewGuid(), DateTime.Now.AddHours(-1));
        }

        protected string DbName;
        private EfCacheClient _sut;
        private string _knownCacheKey;
        private object _knownCacheObject;
        private string _expiredCacheKey;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            DbName = GetType().Name + "-" + Guid.NewGuid();
            using (var context = new CachingDbContext(DbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            using (var context = new CachingDbContext(DbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();
            }
        }


        [Test]
        public void when_cache_item_added()
        {
            var result = _sut.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            Assert.IsTrue(result);
        }

        [Test]
        public void when_cache_item_added_with_same_key()
        {
            var result = _sut.Add(_knownCacheKey, Guid.NewGuid().ToString());

            Assert.IsFalse(result);
        }

        [Test]
        public void when_getting_expired_item_from_cache()
        {
            var item = _sut.Get<Guid>(_expiredCacheKey);
            Assert.AreEqual(Guid.Empty, item);
        }

        [Test]
        public void when_getting_item_from_cache()
        {
            var item = _sut.Get<Guid>(_knownCacheKey);

            Assert.AreEqual(_knownCacheObject, item);
        }

        [Test]
        public void when_replacing_item_in_cache()
        {
            var expectedValue = Guid.NewGuid();
            _sut.Set(_knownCacheKey, expectedValue);

            var actualValue = _sut.Get<Guid>(_knownCacheKey);

            Assert.AreNotEqual(_knownCacheObject, actualValue);
            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}