﻿using System.Web.Mvc;
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
        public ServiceStackController(ICacheClient cache, IServerSettings serverSettings) 
            : base(serverSettings)
        {
            _cache = cache;
            ViewData["IsAuthenticated"] = AuthSession.IsAuthenticated;
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
