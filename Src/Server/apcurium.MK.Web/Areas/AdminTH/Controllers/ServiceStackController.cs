using System.Web.Mvc;
using System.Web.Routing;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints.Extensions;
using apcurium.MK.Common.Configuration;
using System;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class ServiceStackController : BaseController
    {
        private readonly ICacheClient _cache;
        private object _userSession;

        protected ServiceStackController(ICacheClient cache, IServerSettings serverSettings)
           : base(serverSettings)
        {
            _cache = cache;
            ViewData["IsAuthenticated"] = AuthSession.IsAuthenticated;
        }

        protected IAuthSession AuthSession
        {
            get { return SessionAs<AuthUserSession>(); }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!AuthSession.IsAuthenticated)
            {
                filterContext.Result = Redirect(BaseUrl);
            }
            base.OnActionExecuting(filterContext);
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
