using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
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
            string target = string.Empty;

            var p = new OptionSet()
            {
                {"t|target=", "Target application: ios or android", t => target = t.ToLowerInvariant()}
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                ShowHelpAndExit(e.Message, p);
            }

            try
            {

                var resxResourceFileHandler = new ResxResourceFileHandler(GetFullPath(MasterResxRelativePath));
                switch (target)
                {
                    case "android":
                        var androidResourceFileHandler = new AndroidResourceFileHandler(GetFullPath(AndroidRelativePath), resxResourceFileHandler);
                        androidResourceFileHandler.Save();
                        break;
                    case "ios":
                        var iOSResourceFileHandler = new iOSResourceFileHandler(GetFullPath(iOSRelativePath), resxResourceFileHandler);
                        iOSResourceFileHandler.Save();
                        break;
                    default:
                        throw new InvalidOperationException("Invalid program arguments");
                }
                Console.WriteLine("All good!");
            }
            catch (Exception exception)
            {
                Console.Write("error: ");
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

        public static void ShowHelpAndExit(string message, OptionSet optionSet)
        {
            Console.Error.WriteLine(message);
            optionSet.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }
    }
}
