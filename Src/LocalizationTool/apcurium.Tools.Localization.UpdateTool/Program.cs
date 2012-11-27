using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using Newtonsoft.Json;
using apcurium.Tools.Localization.Android;
using apcurium.Tools.Localization.Resx;
using apcurium.Tools.Localization.iOS;

namespace apcurium.Tools.Localization.UpdateTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string target = string.Empty;
            string source = string.Empty;
            string settings = string.Empty;
            string destination = string.Empty;
            bool backup = false;

            var p = new OptionSet()
            {
                {"t|target=", "Target application: ios or android", t => target = t.ToLowerInvariant()},
                {"m|master=", "Master .resx file path", m => source = m},
                {"d|destination=", "Destination file path", d => destination = d},
                {"s|settings:", "JSON settings file path", s => settings = s},
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
                var resourceManager = new ResourceManager();
                var handler = default(ResourceFileHandlerBase);
                resourceManager.AddSource(new ResxResourceFileHandler(source));

                if (settings != null && File.Exists(settings))
                {
                    dynamic appSettings = JsonConvert.DeserializeObject(File.ReadAllText(settings));
                    // Custom resource file should be in the same folder as Master.resx
                    // Name of the custom resource file is equal to settings ApplicationName
                    var customResourcesFilePath = Path.Combine(Path.GetDirectoryName(source), (string)appSettings.ApplicationName + ".resx");
                    if (File.Exists(customResourcesFilePath))
                    {
                        resourceManager.AddSource(new ResxResourceFileHandler(customResourcesFilePath));
                    }
                }

                switch (target)
                {
                    case "android":
                        resourceManager.AddDestination(handler = new AndroidResourceFileHandler(destination));
                        break;
                    case "ios":
                        resourceManager.AddDestination(handler = new iOSResourceFileHandler(destination));
                        break;
                    default:
                        throw new InvalidOperationException("Invalid program arguments");
                }

                resourceManager.Update();
                handler.Save(backup);

                Console.WriteLine("Localization tool ran successfully.");
            }
            catch (Exception exception)
            {
                Console.Write("error: ");
                Console.WriteLine(exception.ToString());
            }
        }

        public static void ShowHelpAndExit(string message, OptionSet optionSet)
        {
            Console.Error.WriteLine(message);
            optionSet.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }
    }
}
