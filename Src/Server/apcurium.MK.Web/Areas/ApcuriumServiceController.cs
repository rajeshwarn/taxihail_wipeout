using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Booking.Api.Extensions;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Http;
using apcurium.MK.Web.Attributes;

namespace apcurium.MK.Web.Areas
{
    public class ApcuriumServiceController : BaseController
    {
        private readonly ICacheClient _cache;
        private object _userSession;

        protected ApcuriumServiceController(ICacheClient cache, IServerSettings serverSettings)
            : base(serverSettings)
        {
            _cache = cache;
            ViewData["IsAuthenticated"] = AuthSession != null;
            ViewData["IsSuperAdmin"] = AuthSession.HasPermission(RoleName.SuperAdmin);
            ViewData["IsAdmin"] = AuthSession.HasPermission(RoleName.Admin);
        }

        protected SessionEntity AuthSession
        {
            get
            {
                return SessionAs<SessionEntity>();
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (NeedsToBeAuthenticated(filterContext))
            {
                if (AuthSession != null)
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
                var cookie = Request.Cookies.Get("ss-pid");

                if (cookie != null)
                {
                    _userSession = _cache.Get<TUserSession>(cookie.Value);
                }
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