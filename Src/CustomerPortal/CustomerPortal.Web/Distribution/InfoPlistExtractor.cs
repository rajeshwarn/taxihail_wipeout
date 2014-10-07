#region

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

#endregion

namespace CustomerPortal.Web.Distribution
{
    public class InfoPlistExtractor
    {
        private const string DefaultPlutilProcessLocation =
            "C:\\Program Files (x86)\\Common Files\\Apple\\Apple Application Support\\plutil.exe";

        private readonly string _plutilProcessLocation;

        public InfoPlistExtractor()
        {
            _plutilProcessLocation = ConfigurationManager.AppSettings["plutil"];
            if (string.IsNullOrEmpty(_plutilProcessLocation))
            {
                _plutilProcessLocation = DefaultPlutilProcessLocation;
            }
        }

        public XDocument ExtractFromPackage(string filepath)
        {
            using (var stream = File.OpenRead(filepath))
            {
                return ExtractFromPackage(stream);
            }
        }

        public XDocument ExtractFromPackage(Stream stream)
        {
            var archive = new ZipArchive(stream);

            var zipEntry =
                archive.Entries.Single(x => string.Equals(x.Name, "Info.plist", StringComparison.OrdinalIgnoreCase));

            var tempFilePath = Path.GetTempFileName();
            using (var tempFile = File.OpenWrite(tempFilePath))
            using (var zipStream = zipEntry.Open())
            {
                zipStream.CopyTo(tempFile);
            }

            // Now use the plutil to convert binary file to xml
            var process = Process.Start(_plutilProcessLocation, "-convert xml1 \"" + tempFilePath + "\"");

            process.WaitForExit();

            var xml = XDocument.Load(tempFilePath);

            // Some cleanup before exiting
            File.Delete(tempFilePath);

            return xml;
        }
    }
}