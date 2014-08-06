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

            var supportedLanguages = new string[] { "", "fr", "ar" };
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

                foreach (var lang in supportedLanguages)
                {

                    var resourceManager = new ResourceManager();
                    var handler = default(ResourceFileHandlerBase);                    
                    
                    resourceManager.AddSource(new ResxResourceFileHandler(AddLanguageToPathResx(source, lang )));


                    if (settings != null && File.Exists(settings))
                    {
                        dynamic appSettings = JsonConvert.DeserializeObject(File.ReadAllText(settings));
                        // Custom resource file should be in the same folder as Master.resx
                        // Name of the custom resource file is equal to settings ApplicationName
                        var customResourcesFilePath = AddLanguageToPathResx(Path.Combine(Path.GetDirectoryName(source), (string)appSettings.ApplicationName + ".resx"), lang);

                        if (File.Exists(customResourcesFilePath))
                        {
                            resourceManager.AddSource(new ResxResourceFileHandler(customResourcesFilePath));
                        }
                    }

                    switch (target)
                    {
                        case "android":
                            
                            resourceManager.AddDestination(handler = new AndroidResourceFileHandler(destination, lang ));
                            break;
                        case "ios":
                            resourceManager.AddDestination(handler = new iOSResourceFileHandler(destination, lang ));
                            break;
                        default:
                            throw new InvalidOperationException("Invalid program arguments");
                    }

                    resourceManager.Update();
                    handler.Save(backup);

                    var l = string.IsNullOrWhiteSpace(lang) ? "en" : lang;
                    Console.WriteLine("Localization for :  " + l + " was successful");

                }

                Console.WriteLine("Localization tool ran successfully.");
            }
            catch (Exception exception)
            {
                Console.Write("error: ");
                Console.WriteLine(exception.ToString());
            }
        }

        private static string AddLanguageToPathResx(string source, string lang)
        {
            if (  ( string.IsNullOrWhiteSpace(lang )) || (!source.ToLower().EndsWith(".resx")))
            {
                return source;
            }



            
            var firstPart = source.Substring(0, source.Length - 5);
            
            return firstPart + "." + lang + ".resx";
        }

        public static void ShowHelpAndExit(string message, OptionSet optionSet)
        {
            Console.Error.WriteLine(message);
            optionSet.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }
    }
}
