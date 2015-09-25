using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Attributes;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints.Extensions;

namespace apcurium.MK.Web.Areas
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
            ViewData["IsSuperAdmin"] = AuthSession.HasPermission(RoleName.SuperAdmin);
            ViewData["IsSupport"] = AuthSession.HasPermission(RoleName.Support);
        }

        protected IAuthSession AuthSession
        {
            get { return SessionAs<AuthUserSession>(); }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (NeedsToBeAuthenticated(filterContext))
            {
                if (!AuthSession.IsAuthenticated)
                {
                    filterContext.Result = Redirect(BaseUrl);
                }
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

        public bool UserHasPermission(IEnumerable<string> permissions)
        {
            return permissions.Any(x => AuthSession.HasPermission(x));
        }

        private bool NeedsToBeAuthenticated(ActionExecutingContext filterContext)
        {
            var skipAuthOnAction = filterContext.ActionDescriptor.GetCustomAttributes(typeof (SkipAuthenticationAttribute), true);
            var skipAuthOnController = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(SkipAuthenticationAttribute), true);
            
            return !(skipAuthOnAction.Any() || skipAuthOnController.Any());
        }
    }
}