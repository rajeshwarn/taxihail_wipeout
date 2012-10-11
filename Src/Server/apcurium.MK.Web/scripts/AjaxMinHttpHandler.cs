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

            var source = Path.GetFileNameWithoutExtension(context.Request.FilePath);
            source = source.Replace("minified.", "");
            context.Response.AddHeader("Content-Type", "application/javascript");
            var scripts = XDocument.Load(HostingEnvironment.MapPath("~/scripts/" + source + ".xml"));

            var files = scripts.Root.Elements("script")
                .Select(x=> (string)x.Attribute("src"))
                .Select(x => x.StartsWith("/") ? x : "~/" + x)
                .Select(HostingEnvironment.MapPath)
                .ToArray();

            context.Response.AddFileDependencies(files);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(10080.0));
            context.Response.Cache.SetETagFromFileDependencies();
            context.Response.Cache.SetLastModifiedFromFileDependencies();
            context.Response.Cache.SetVaryByCustom("Accept-Encoding");

            var minifier = new Minifier();
            foreach (var path in files)
            {
                var content = File.ReadAllText(path);
                context.Response.Write(";" + minifier.MinifyJavaScript(content));
            }

        }

        #endregion
    }
}
