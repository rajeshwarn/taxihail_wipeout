using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace apcurium.MK.Booking.Email
{
    public class TemplateService : ITemplateService
    {
        public string Find(string templateName)
        {
            var path = Path.Combine(AssemblyDirectory, "Email\\Templates", templateName + ".html");
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return null;
        }

        public string Render(string template, object data)
        {
            if(template == null) throw new ArgumentNullException("template");
            return Nustache.Core.Render.StringToString(template, data);
        }

        public string ImagePath(string imageName)
        {
            return Path.Combine(AssemblyDirectory, "Email\\Images", imageName);
        }

        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
