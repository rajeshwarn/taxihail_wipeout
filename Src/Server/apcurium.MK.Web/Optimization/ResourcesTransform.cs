using System.IO;
using System.Text;
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
                content.Append("TaxiHail.resources['" + file.VirtualFile.Name.Replace(".json", "") + "'] = ");
                using (var reader = new StreamReader(file.VirtualFile.Open(), Encoding.UTF8))
                {
                    content.Append(reader.ReadToEnd() + ';');
                }
                response.Content = content.ToString();
            }
        }
    }
}