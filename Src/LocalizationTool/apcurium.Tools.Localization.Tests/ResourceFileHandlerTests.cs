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
using apcurium.Tools.Localization.Resx;
using apcurium.Tools.Localization.iOS;

namespace apcurium.Tools.Localization.Tests
{
    public class ResourceFileHandlerTests
    {
        const string AndroidRelativePath = @"\mk-taxi\Src\Mobile\Android\Resources\Values\String.xml";
        const string iOSRelativePath = @"\mk-taxi\Src\Mobile\iOS\en.lproj\Localizable.strings";
        const string MasterResxRelativePath = @"\mk-taxi\Src\LocalizationTool\Master.resx";

        [Test]
        public void AndroidResourceFileHandler()
        {
            var fullPath = GetFullPath(AndroidRelativePath);

            var androidResourceFileHandler = new AndroidResourceFileHandler(fullPath, "");

            Assert.That(androidResourceFileHandler.Count, Is.GreaterThan(0));

            string backupFilePath = androidResourceFileHandler.Save();
            var backupAndroidResourceFileHandler = new AndroidResourceFileHandler(backupFilePath, "");
            Assert.That(backupAndroidResourceFileHandler.Count, Is.EqualTo(androidResourceFileHandler.Count));

            androidResourceFileHandler.Add("maxime", "allo");
            androidResourceFileHandler.Save(false);
            androidResourceFileHandler = new AndroidResourceFileHandler(fullPath, "");
            Assert.That(androidResourceFileHandler.Count, Is.EqualTo(backupAndroidResourceFileHandler.Count + 1));

            File.Copy(backupFilePath, fullPath, true);
            File.Delete(backupFilePath);
        }

        [Test]
        public void IosResourceFileHandler()
        {
            var fullPath = GetFullPath(iOSRelativePath);

            var iOSResourceFileHandler = new iOSResourceFileHandler(fullPath, "");

            Assert.That(iOSResourceFileHandler.Count, Is.GreaterThan(0));

            string backupFilePath = iOSResourceFileHandler.Save();
            var backupIosResourceFileHandler = new iOSResourceFileHandler(backupFilePath, "");
            Assert.That(backupIosResourceFileHandler.Count, Is.EqualTo(iOSResourceFileHandler.Count));

            iOSResourceFileHandler.Add("maxime", "allo");
            iOSResourceFileHandler.Save(false);
            iOSResourceFileHandler = new iOSResourceFileHandler(fullPath, "");
            Assert.That(iOSResourceFileHandler.Count, Is.EqualTo(backupIosResourceFileHandler.Count + 1));

            File.Copy(backupFilePath, fullPath, true);
            File.Delete(backupFilePath);
        }


        [Test]
        public void CompareMasterResxWithClientResourceFiles()
        {
            var resxResourceFileHandler = new ResxResourceFileHandler(GetFullPath(MasterResxRelativePath));
            var iOSResourceFileHandler = new iOSResourceFileHandler(GetFullPath(iOSRelativePath), "");
            var androidResourceFileHandler = new AndroidResourceFileHandler(GetFullPath(AndroidRelativePath), "");

            var androidResourceFileHandlerDiff = CompareResourceFileHandlers(resxResourceFileHandler, androidResourceFileHandler);
            var iOSResourceFileHandlerDiff = CompareResourceFileHandlers(resxResourceFileHandler, iOSResourceFileHandler);

            Assert.That(iOSResourceFileHandlerDiff.GetProblematicKeys().Count(), Is.EqualTo(0));
            Assert.That(androidResourceFileHandlerDiff.GetProblematicKeys().Count(), Is.EqualTo(0));

            //if (iOSResourceFileHandlerDiff.GetProblematicKeys().Any())
            //{
            //    var report = BuildCompareReportCsv(iOSResourceFileHandlerDiff);
            //}

            //if (androidResourceFileHandlerDiff.GetProblematicKeys().Any())
            //{
            //    var report = BuildCompareReportCsv(androidResourceFileHandlerDiff);
            //}
        }

        #region Tools

        [Test]
        [Ignore]
        public void BuildMasterResx()
        {
            var iOSResourceFileHandler = new iOSResourceFileHandler(GetFullPath(iOSRelativePath),  "");
            var androidResourceFileHandler = new AndroidResourceFileHandler(GetFullPath(AndroidRelativePath), "");

            var resourceFileHandlerDiff = CompareResourceFileHandlers(iOSResourceFileHandler, androidResourceFileHandler);

            if (!resourceFileHandlerDiff.Diff.Any())
            {
                using (var rw = new ResXResourceWriter(GetFullPath(MasterResxRelativePath)))
                {
                    foreach (var key in resourceFileHandlerDiff.Keys.Except(resourceFileHandlerDiff.GetProblematicKeys()))
                    {
                        rw.AddResource(key, iOSResourceFileHandler[key]);
                    }

                    foreach (var androidMissgingKey in resourceFileHandlerDiff.RightResourceFileHandlerMissgingKeys)
                    {
                        rw.AddResource(androidMissgingKey, iOSResourceFileHandler[androidMissgingKey]);
                    }

                    foreach (var iOSMissgingKey in resourceFileHandlerDiff.LeftResourceFileHandlerMissgingKeys)
                    {
                        rw.AddResource(iOSMissgingKey, androidResourceFileHandler[iOSMissgingKey]);
                    }
                }
            }

            var report = BuildCompareReportCsv(resourceFileHandlerDiff);
        }

        [Test]
        [Ignore]
        public void UpdateAndroidClientResourceFiles()
        {
            var resxResourceFileHandler = new ResxResourceFileHandler(GetFullPath(MasterResxRelativePath));

            var androidResourceFileHandler = new AndroidResourceFileHandler(GetFullPath(AndroidRelativePath), resxResourceFileHandler, "");

            androidResourceFileHandler.Save();
        }

        [Test]
        [Ignore]
        public void UpdateIosClientResourceFiles()
        {
            var resxResourceFileHandler = new ResxResourceFileHandler(GetFullPath(MasterResxRelativePath));
            var iOSResourceFileHandler = new iOSResourceFileHandler(GetFullPath(iOSRelativePath), resxResourceFileHandler, "");

            iOSResourceFileHandler.Save();
        }

        #endregion

        #region Utilities

        private string BuildCompareReportCsv(ResourceFileHandlerDiff resourceFileHandlerDiff)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("\"key\",\"iOS\",\"Android\"\n");

            foreach (var problematicKey in resourceFileHandlerDiff.Diff)
            {
                stringBuilder.AppendFormat("\"{0}\",\"{1}\",\"{2}\"\n", problematicKey.Key, problematicKey.Value.Item1, problematicKey.Value.Item2);
            }

            return stringBuilder.ToString();
        }

        private ResourceFileHandlerDiff CompareResourceFileHandlers(ResourceFileHandlerBase leftResourceFileHandler, ResourceFileHandlerBase rightResourceFileHandler)
        {
            var resourceFileHandlerDiff = new ResourceFileHandlerDiff
                {
                    Keys = new HashSet<string>(leftResourceFileHandler.Select(e => e.Key).Union(rightResourceFileHandler.Select(e => e.Key)).Distinct(StringComparer.OrdinalIgnoreCase))
                };

            foreach (var key in resourceFileHandlerDiff.Keys)
            {
                string leftResourceValue = null;
                string rightResourceValue = null;

                leftResourceFileHandler.TryGetValue(key, out leftResourceValue);
                rightResourceFileHandler.TryGetValue(key, out rightResourceValue);

                if (leftResourceValue == null)
                {
                    resourceFileHandlerDiff.LeftResourceFileHandlerMissgingKeys.Add(key);
                }
                else if (rightResourceValue == null)
                {
                    resourceFileHandlerDiff.RightResourceFileHandlerMissgingKeys.Add(key);
                }
                else if (leftResourceValue != rightResourceValue)
                {
                    resourceFileHandlerDiff.Diff.Add(key, new Tuple<string, string>(leftResourceValue, rightResourceValue));
                }
            }

            return resourceFileHandlerDiff;
        }

        private string GetFullPath(string relativePath)
        {
            var basePath = GetCurrentAssemblyExecutingPath();
            var index = basePath.IndexOf(@"\mk-taxi\Src\", StringComparison.OrdinalIgnoreCase);
            basePath = basePath.Substring(0, index);

            return relativePath.StartsWith("\\") ? basePath + relativePath : basePath + "\\" + relativePath;
        }

        private static string GetCurrentAssemblyExecutingPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }

        public class ResourceFileHandlerDiff
        {
            public ResourceFileHandlerDiff()
            {
            
                Diff = new Dictionary<string, Tuple<string, string>>();
                LeftResourceFileHandlerMissgingKeys = new HashSet<string>();
                RightResourceFileHandlerMissgingKeys = new HashSet<string>();
                Keys = new HashSet<string>();
            }

            public IEnumerable<string> GetProblematicKeys( )
            {
                return LeftResourceFileHandlerMissgingKeys.Union(RightResourceFileHandlerMissgingKeys).Union(Diff.Select(v => v.Key)).Distinct();
            }

            public HashSet<string> Keys { get; set; }
            public Dictionary<string, Tuple<string, string>> Diff { get; set; }
            public HashSet<string> LeftResourceFileHandlerMissgingKeys { get; set; }
            public HashSet<string> RightResourceFileHandlerMissgingKeys { get; set; }
        }

        #endregion
    }
}
