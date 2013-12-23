using System.IO;
using System.Text;
using System.Web.Optimization;

namespace apcurium.MK.Web.Optimization
{
    public class HandlebarsTransform: IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            response.ContentType = "application/javascript";
            var content = new StringBuilder("Handlebars.templates = {};");
            foreach (var file in response.Files)
            {
// ReSharper disable once PossibleNullReferenceException
                content.Append("Handlebars.templates['" + file.VirtualFile.Name.Replace(".handlebars", "") + "'] = '");
                using(var reader = new StreamReader(file.VirtualFile.Open(), Encoding.UTF8))
                {
                    content.Append(reader.ReadToEnd().Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n") + "';");
                }
            }
            response.Content = content.ToString();
        }
    }
}