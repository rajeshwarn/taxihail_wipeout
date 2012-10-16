using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace apcurium.MK.Web.Handlebars
{
    public class HandlebarsHttpHandler : IHttpHandler
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
            var filepath = HostingEnvironment.MapPath(context.Request.Url.PathAndQuery);
            var directory = Path.GetDirectoryName(filepath);
            var files = Directory.EnumerateFiles(directory, "*.handlebars").ToArray();
            context.Response.AddFileDependencies(files);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(10080.0));
            context.Response.Cache.SetETagFromFileDependencies();
            context.Response.Cache.SetLastModifiedFromFileDependencies();
            context.Response.Cache.SetVaryByCustom("Accept-Encoding");

            context.Response.AddHeader("Content-Type", "application/javascript");
            context.Response.Write("Handlebars.templates = {};");
            foreach (var filePath in files)
            {
                var file = new FileInfo(filePath);
                context.Response.Write("Handlebars.templates['" + file.Name.Replace(".handlebars", "") + "'] = '" + File.ReadAllText(filePath).Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n") + "';");
            }

        }

        #endregion


    }
}
