using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using apcurium.MK.Booking.Api.Extensions;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
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
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var authSession = SessionAs<SessionEntity>(requestContext.HttpContext.Request);

            ViewData["IsAuthenticated"] = authSession != null;
            ViewData["IsSuperAdmin"] = authSession.HasPermission(RoleName.SuperAdmin);
            ViewData["IsAdmin"] = authSession.HasPermission(RoleName.Admin);
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
                if (!AuthSession.IsAuthenticated())
                {
                    filterContext.Result = Redirect(BaseUrl);
                }
            }
            
            base.OnActionExecuting(filterContext);
        }

        protected TUserSession SessionAs<TUserSession>()
        {
            return SessionAs<TUserSession>(Request);
        }

        protected TUserSession SessionAs<TUserSession>(HttpRequestBase request)
        {
            if (_userSession == null)
            {
                var sessionId = request.Cookies.Get("ss-pid")
                    .SelectOrDefault(cookie => Uri.UnescapeDataString(cookie.Value));

                if (sessionId.HasValueTrimmed())
                {
                    _userSession = _cache.Get<TUserSession>("urn:iauthsession:" + sessionId);
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