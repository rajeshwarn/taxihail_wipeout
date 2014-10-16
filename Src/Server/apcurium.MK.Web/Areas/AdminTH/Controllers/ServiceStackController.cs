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
        private readonly IServerSettings _serverSettings;
        public ServiceStackController(ICacheClient cache, IServerSettings serverSettings)
        {
            _cache = cache;
            _serverSettings = serverSettings;
            ViewData["ApplicationName"] = serverSettings.ServerData.TaxiHail.ApplicationName;
            ViewData["ApplicationKey"] = serverSettings.ServerData.TaxiHail.ApplicationKey;
            ViewData["IsAuthenticated"] = AuthSession.IsAuthenticated;
            BaseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath;
            ViewData["BaseUrl"] = BaseUrl;

        }
        public string BaseUrl { get; set; }
        protected IAuthSession AuthSession
        {
            get { return SessionAs<AuthUserSession>(); }
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
