using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Optimization;

namespace apcurium.MK.Web.Optimization
{
    public class ResourcesTransform: IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            response.ContentType = "application/javascript";
            var content = new StringBuilder("TaxiHail.resources = {};");
            foreach (var file in response.Files)
            {
                content.Append("TaxiHail.resources['" + file.Name.Replace(".json", "") + "'] = ");
                using (var reader = file.OpenText())
                {
                    content.Append(reader.ReadToEnd() + ';');
                }
                response.Content = content.ToString();
            }
        }
    }
}