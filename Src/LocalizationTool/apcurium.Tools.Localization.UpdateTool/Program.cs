using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using apcurium.Tools.Localization.Android;
using apcurium.Tools.Localization.Resx;
using apcurium.Tools.Localization.iOS;

namespace apcurium.Tools.Localization.UpdateTool
{
    class Program
    {
        const string AndroidRelativePath = @"\mk-taxi\Src\Mobile\Android\Resources\Values\String.xml";
        const string iOSRelativePath = @"\mk-taxi\Src\Mobile\iOS\en.lproj\Localizable.strings";
        const string MasterResxRelativePath = @"\mk-taxi\Src\LocalizationTool\Master.resx";

        static void Main(string[] args)
        {
            try
            {
                var resxResourceFileHandler = new ResxResourceFileHandler(GetFullPath(MasterResxRelativePath));
                var androidResourceFileHandler = new AndroidResourceFileHandler(GetFullPath(AndroidRelativePath), resxResourceFileHandler);
                var iOSResourceFileHandler = new iOSResourceFileHandler(GetFullPath(iOSRelativePath), resxResourceFileHandler);

                androidResourceFileHandler.Save();
                iOSResourceFileHandler.Save();

                Console.WriteLine("All good!");
                Console.ReadKey();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private static string GetFullPath(string relativePath)
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
    }
}
