using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace apcurium.MK.Web
{
    public class PageBase : Page
    {
        private Container container;

        public Container Container
        {
            get { return container ?? (container = AppHostBase.Instance.Container); }
        }

        protected string SessionKey
        {
            get
            {
                var sessionId = SessionFeature.GetSessionId();
                return sessionId == null ? null : SessionFeature.GetSessionKey(sessionId);
            }
        }

        private AuthUserSession userSession;

        protected AuthUserSession UserSession
        {
            get
            {
                if (userSession != null) return userSession;
                if (SessionKey != null)
                    userSession = this.Cache.Get<AuthUserSession>(SessionKey);
                else
                    SessionFeature.CreateSessionIds();

                var unAuthorizedSession = new AuthUserSession();
                return userSession ?? (userSession = unAuthorizedSession);
            }
        }

        public void ClearSession()
        {
            userSession = null;
            this.Cache.Remove(SessionKey);
        }

        public new ICacheClient Cache
        {
            get { return Container.Resolve<ICacheClient>(); }
        }

        public ISessionFactory SessionFactory
        {
            get { return Container.Resolve<ISessionFactory>(); }
        }

        private ISession session;

        public new ISession Session
        {
            get { return session ?? (session = SessionFactory.GetOrCreateSession()); }
        }
    }
}