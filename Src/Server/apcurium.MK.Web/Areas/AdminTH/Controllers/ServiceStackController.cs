using System.Web.Mvc;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints.Extensions;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class ServiceStackController : Controller
    {
        private readonly ICacheClient _cache;

        public ServiceStackController(ICacheClient cache)
        {
            _cache = cache;
        }
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
