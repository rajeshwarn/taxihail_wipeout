#region

using System.Linq;
using CustomerPortal.Web.Distribution;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Distribution
{
    [TestFixture]
    public class IpaInfoPlistExtractorFixture
    {
        [Test]
        public void CanReadBundleIdentifierFromPackage()
        {
            var sut = new InfoPlistExtractor();
            var stream = GetType().Assembly.GetManifestResourceStream("CustomerPortal.Web.Test.Resources.package.ipa");
            var xml = sut.ExtractFromPackage(stream);

            var bundleIdentifier = xml.Root.Descendants("key")
                .First(x => x.Value == "CFBundleIdentifier")
                .ElementsAfterSelf("string").First().Value;

            Assert.AreEqual("com.apcurium.MK.CentralTaxiNiagara", bundleIdentifier);
        }
    }
}