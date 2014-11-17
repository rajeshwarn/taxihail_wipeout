using System.Web.Mvc;
using System.Web.Routing;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints.Extensions;
using apcurium.MK.Common.Configuration;
using System;
using System.Web;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class ServiceStackController : Controller
    {
        private readonly ICacheClient _cache;
        public ServiceStackController(ICacheClient cache, IServerSettings serverSettings)
        {
            _cache = cache;
            ViewData["ApplicationName"] = serverSettings.ServerData.TaxiHail.ApplicationName;
            ViewData["ApplicationKey"] = serverSettings.ServerData.TaxiHail.ApplicationKey;
            ViewData["IsAuthenticated"] = AuthSession.IsAuthenticated;
        }

        public RequestContext Context;

        public string BaseUrl { get; set; }
        
        public string SessionID { get; set; }

        public string BaseUrlAPI
        {
            get { return BaseUrl + "/api/"; }
        }

        protected IAuthSession AuthSession
        {
            get { return SessionAs<AuthUserSession>(); }
        }

        protected override IAsyncResult BeginExecute(System.Web.Routing.RequestContext requestContext, AsyncCallback callback, object state)
        {
            Context = requestContext;
            BaseUrl = Context.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + requestContext.HttpContext.Request.ApplicationPath;
            ViewData["BaseUrl"] = BaseUrl;
            var sessionCookie = Context.HttpContext.Request.Cookies["ss-pid"];
            SessionID = sessionCookie != null ? sessionCookie.Value : "";
            return base.BeginExecute(requestContext, callback, state);
        }

        private object _userSession;

        protected TUserSession SessionAs<TUserSession>()
        {
            if (_userSession == null)
            {
                _userSession = _cache.SessionAs<TUserSession>(System.Web.HttpContext.Current.Request.ToRequest());
            }
            return (TUserSession)_userSession;
        }


    }
}
