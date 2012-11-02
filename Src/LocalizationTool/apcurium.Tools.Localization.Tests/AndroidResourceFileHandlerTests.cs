using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using apcurium.Tools.Localization.Android;

namespace apcurium.Tools.Localization.Tests
{
    public class AndroidResourceFileHandlerTests
    {
        [Test]
        public void TestX()
        {
            const string relativeFilePath = @"\mk-taxi\Src\Mobile\Android\Resources\Values\String.xml";
            string basePath = GetCurrentAssemblyExecutingPath();
            var index = basePath.IndexOf(@"\mk-taxi\Src\", StringComparison.OrdinalIgnoreCase);
            basePath = basePath.Substring(0, index);

            var androidResourceFileHandler = new AndroidResourceFileHandler(basePath +relativeFilePath);

            Assert.That(androidResourceFileHandler.Count, Is.GreaterThan(0));
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
