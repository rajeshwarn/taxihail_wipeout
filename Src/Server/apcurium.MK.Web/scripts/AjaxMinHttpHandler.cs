using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using Microsoft.Ajax.Utilities;

namespace apcurium.MK.Web.Scripts
{
    public class AjaxMinHttpHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var source = context.Request.QueryString["source"];
            context.Response.AddHeader("Content-Type", "application/javascript");
            var scripts = XDocument.Load(HostingEnvironment.MapPath("~/scripts/" + source + ".xml"));

            var minifier = new Minifier();
            foreach (var script in scripts.Root.Elements("script"))
            {
                var filePath = (string) script.Attribute("src");
                if (!filePath.StartsWith("/")) filePath = "~/" + filePath;
                filePath = HostingEnvironment.MapPath(filePath);
                var content = File.ReadAllText(filePath);
                context.Response.Write(";" + minifier.MinifyJavaScript(content));
            }

        }

        #endregion
    }
}
