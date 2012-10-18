using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
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
                content.Append("Handlebars.templates['" + file.Name.Replace(".handlebars", "") + "'] = '");
                using(var reader = file.OpenText())
                {
                    content.Append(reader.ReadToEnd().Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n") + "';");
                }
            }
            response.Content = content.ToString();
        }
    }
}