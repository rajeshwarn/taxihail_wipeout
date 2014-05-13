#region

using System;
using System.IO;
using System.Reflection;

#endregion

namespace apcurium.MK.Booking.Email
{
    public class TemplateService : ITemplateService
    {
        private const string DefaultLanguageCode = "en";

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public string Find(string templateName, string languageCode = DefaultLanguageCode)
        {
            var path = Path.Combine(AssemblyDirectory, "Email\\Templates", languageCode, templateName + ".html");
            if (!File.Exists(path))
            {
                path = Path.Combine(AssemblyDirectory, "Email\\Templates", DefaultLanguageCode, templateName + ".html");
            }

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return null;
        }

        public string Render(string template, object data)
        {
            if (template == null) throw new ArgumentNullException("template");
            return Nustache.Core.Render.StringToString(template, data);
        }

        public string ImagePath(string imageName)
        {
            return Path.Combine(AssemblyDirectory, "Email\\Images", imageName);
        }
    }
}