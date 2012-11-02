using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using apcurium.Tools.Localization.Android;
using apcurium.Tools.Localization.iOS;

namespace apcurium.Tools.Localization.Tests
{
    public class ResourceFileHandlerTests
    {
        [Test]
        public void AndroidResourceFileHandler()
        {
            const string relativePath = @"\mk-taxi\Src\Mobile\Android\Resources\Values\String.xml";

            var androidResourceFileHandler = new AndroidResourceFileHandler(GetFullPath(relativePath));

            Assert.That(androidResourceFileHandler.Count, Is.GreaterThan(0));
            Assert.That(androidResourceFileHandler.DuplicateKeys.Count, Is.EqualTo(0));
        }

        [Test]
        public void iOSResourceFileHandler()
        {
            const string relativePath = @"\mk-taxi\Src\Mobile\iOS\en.lproj\Localizable.strings";

            var iOSResourceFileHandler = new iOSResourceFileHandler(GetFullPath(relativePath));

            Assert.That(iOSResourceFileHandler.Count, Is.GreaterThan(0));
            Assert.That(iOSResourceFileHandler.DuplicateKeys.Count, Is.EqualTo(0));
        }

        private string GetFullPath(string relativePath)
        {
            string basePath = GetCurrentAssemblyExecutingPath();
            var index = basePath.IndexOf(@"\mk-taxi\Src\", StringComparison.OrdinalIgnoreCase);
            basePath = basePath.Substring(0, index);

            return basePath + relativePath;
        }

        private static string GetCurrentAssemblyExecutingPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
