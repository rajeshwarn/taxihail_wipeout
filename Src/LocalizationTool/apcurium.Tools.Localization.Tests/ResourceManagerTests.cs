using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace apcurium.Tools.Localization.Tests
{
    [TestFixture]
    public class ResourceManagerTests
    {
        private const string SampleKey = "SampleKey";
        private const string SampleValue = "SampleValue";
        private ResourceManager sut;
        private FakeResourceFileHandler sourceFileHandler;
        private FakeResourceFileHandler destinationFileHandler;

        [SetUp]
        public void SetUp()
        {
            sourceFileHandler = new FakeResourceFileHandler
            {
                {SampleKey, SampleValue}
            };

            destinationFileHandler = new FakeResourceFileHandler();

            sut = new ResourceManager();
            sut.AddSource(sourceFileHandler);
            sut.AddDestination(destinationFileHandler);
        }


        [Test]
        public void when_adding_null_source()
        {
            Assert.Throws<ArgumentNullException>(() => sut.AddSource(default(ResourceFileHandlerBase)));
        }

        [Test]
        public void when_adding_null_destination()
        {
            Assert.Throws<ArgumentNullException>(() => sut.AddDestination(default(ResourceFileHandlerBase)));
        }

        [Test]
        public void when_updating()
        {
            sut.Update();

            Assert.IsTrue(destinationFileHandler.ContainsKey(SampleKey));
            Assert.AreEqual(SampleValue, destinationFileHandler[SampleKey]);


        }

        private class FakeResourceFileHandler : ResourceFileHandlerBase
        {
            public FakeResourceFileHandler(): base(null)
            {
            }

            public FakeResourceFileHandler(string filePath) : base(filePath)
            {
            }

            public FakeResourceFileHandler(string filePath, IDictionary<string, string> dictionary) : base(filePath, dictionary)
            {
            }

            protected override string GetFileText()
            {
                throw new NotImplementedException();
            }
        }


    }
}
