using System.Web.Mvc;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints.Extensions;
using apcurium.MK.Common.Configuration;
using System;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class ServiceStackController : Controller
    {
        private readonly ICacheClient _cache;
        private object _userSession;

        protected ServiceStackController(ICacheClient cache, IServerSettings serverSettings)
        {
            _cache = cache;
            ViewData["ApplicationName"] = serverSettings.ServerData.TaxiHail.ApplicationName;
            ViewData["ApplicationKey"] = serverSettings.ServerData.TaxiHail.ApplicationKey;
            ViewData["IsAuthenticated"] = AuthSession.IsAuthenticated;
        }

        public string BaseUrl { get; set; }

        protected IAuthSession AuthSession
        {
            get { return SessionAs<AuthUserSession>(); }
        }

        protected override IAsyncResult BeginExecute(System.Web.Routing.RequestContext requestContext, AsyncCallback callback, object state)
        {
            BaseUrl = requestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + requestContext.HttpContext.Request.ApplicationPath;
            ViewData["BaseUrl"] = BaseUrl;
            return base.BeginExecute(requestContext, callback, state);
        }

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
