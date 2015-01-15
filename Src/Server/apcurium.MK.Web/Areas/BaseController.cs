using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.Areas
{
    public class BaseController : Controller
    {
        public string BaseUrl { get; set; }

        public BaseController(IServerSettings serverSettings)
        {
            ViewData["ApplicationName"] = serverSettings.ServerData.TaxiHail.ApplicationName;
            ViewData["ApplicationKey"] = serverSettings.ServerData.TaxiHail.ApplicationKey;
            ViewData["IsTaxiHailPro"] = serverSettings.ServerData.IsTaxiHailPro;
            ViewData["Version"] = Assembly.GetAssembly(typeof (_default)).GetName().Version.ToString();
        }

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            BaseUrl = requestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + requestContext.HttpContext.Request.ApplicationPath;
            ViewData["BaseUrl"] = BaseUrl;
            return base.BeginExecute(requestContext, callback, state);
        }
    }
}