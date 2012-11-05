using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using apcurium.Tools.Localization.Android;
using apcurium.Tools.Localization.iOS;

namespace apcurium.Tools.Localization.Tests
{
    public class ResourceFileHandlerTests
    {
        const string AndroidRelativePath = @"\mk-taxi\Src\Mobile\Android\Resources\Values\String.xml";
        const string iOSRelativePath = @"\mk-taxi\Src\Mobile\iOS\en.lproj\Localizable.strings";

        [Test]
        public void AndroidResourceFileHandler()
        {
            var fullPath = GetFullPath(AndroidRelativePath);

            var androidResourceFileHandler = new AndroidResourceFileHandler(fullPath);

            Assert.That(androidResourceFileHandler.Count, Is.GreaterThan(0));
            //Assert.That(androidResourceFileHandler.DuplicateKeys.Count, Is.EqualTo(0));

            string backupFilePath = androidResourceFileHandler.Save();
            var backupAndroidResourceFileHandler = new AndroidResourceFileHandler(backupFilePath);
            Assert.That(backupAndroidResourceFileHandler.Count, Is.EqualTo(androidResourceFileHandler.Count));

            androidResourceFileHandler.Add("maxime", "allo");
            androidResourceFileHandler.Save(false);
            androidResourceFileHandler = new AndroidResourceFileHandler(fullPath);
            Assert.That(androidResourceFileHandler.Count, Is.EqualTo(backupAndroidResourceFileHandler.Count + 1));

            File.Copy(backupFilePath, fullPath, true);
            File.Delete(backupFilePath);
        }

        [Test]
        public void IosResourceFileHandler()
        {
            var fullPath = GetFullPath(iOSRelativePath);

            var iOSResourceFileHandler = new iOSResourceFileHandler(fullPath);

            Assert.That(iOSResourceFileHandler.Count, Is.GreaterThan(0));

            string backupFilePath = iOSResourceFileHandler.Save();
            var backupIosResourceFileHandler = new iOSResourceFileHandler(backupFilePath);
            Assert.That(backupIosResourceFileHandler.Count, Is.EqualTo(iOSResourceFileHandler.Count));

            iOSResourceFileHandler.Add("maxime", "allo");
            iOSResourceFileHandler.Save(false);
            iOSResourceFileHandler = new iOSResourceFileHandler(fullPath);
            Assert.That(iOSResourceFileHandler.Count, Is.EqualTo(backupIosResourceFileHandler.Count + 1));

            File.Copy(backupFilePath, fullPath, true);
            File.Delete(backupFilePath);
        }

        [Test]
        public void CompareResourceFileHandlers()
        {
            var iOSResourceFileHandler = new iOSResourceFileHandler(GetFullPath(iOSRelativePath));
            var androidResourceFileHandler = new AndroidResourceFileHandler(GetFullPath(AndroidRelativePath));

            var keys = iOSResourceFileHandler.Select(e => e.Key).Union(androidResourceFileHandler.Select(e => e.Key)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var diff = new Dictionary<string, Tuple<string, string>>();
            var iOSMissgingKeys = new HashSet<string>();
            var androidMissgingKeys = new HashSet<string>();

            foreach (var key in keys)
            {
                string iOSValue = null;
                string androidValue = null;

                iOSResourceFileHandler.TryGetValue(key, out iOSValue);
                androidResourceFileHandler.TryGetValue(key, out androidValue);

                if (iOSValue == null)
                {
                    iOSMissgingKeys.Add(key);
                }
                else if (androidValue == null)
                {
                    androidMissgingKeys.Add(key);
                }
                else if (iOSValue != androidValue)
                {
                    diff.Add(key, new Tuple<string, string>(iOSValue, androidValue));
                }
            }

            var problematicKeys = iOSMissgingKeys.Union(androidMissgingKeys).Union(diff.Select(i => i.Key));

            using (var rw = new ResXResourceWriter(GetFullPath("test.resx")))
            {
                foreach (var key in keys.Except(problematicKeys))
                {
                    rw.AddResource(key, iOSResourceFileHandler[key]);
                }
            }
        }

        #region Utilities

        private string GetFullPath(string relativePath)
        {
            string basePath = GetCurrentAssemblyExecutingPath();
            var index = basePath.IndexOf(@"\mk-taxi\Src\", StringComparison.OrdinalIgnoreCase);
            basePath = basePath.Substring(0, index);

            return relativePath.StartsWith("\\") ? basePath + relativePath : basePath + "\\" + relativePath;
        }

        private static string GetCurrentAssemblyExecutingPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        #endregion
    }
}
