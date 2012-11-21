using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        const string AppSettings = @"\mk-taxi\Src\Mobile\Common\Settings\Settings.json";

        static void Main(string[] args)
        {
            string target = string.Empty;
            string source = string.Empty;
            string destination = string.Empty;
            bool backup = false;

            var p = new OptionSet()
            {
                {"t|target=", "Target application: ios or android", t => target = t.ToLowerInvariant()},
                {"s|source=", "Source .resx file path", s => source = s},
                {"d|destination=", "Destination file path", d => destination = d},
                {"b|backup", "Backup file", b => backup = b != null},

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

                var resxResourceFileHandler = new ResxResourceFileHandler(source);
                switch (target)
                {
                    case "android":
                        var androidResourceFileHandler = new AndroidResourceFileHandler(destination, resxResourceFileHandler);
                        androidResourceFileHandler.Save(backup);
                        break;
                    case "ios":
                        var iOSResourceFileHandler = new iOSResourceFileHandler(destination, resxResourceFileHandler);
                        iOSResourceFileHandler.Save(backup);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid program arguments");
                }
                Console.WriteLine("Localization tool ran successfully.");
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
            Console.WriteLine("basePath: " + basePath);

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
