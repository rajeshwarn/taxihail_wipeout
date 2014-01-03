#region

using System.Web.UI;
using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

#endregion

namespace apcurium.MK.Web
{
    public class PageBase : Page
    {
        private Container _container;
        private ISession _session;
        private AuthUserSession _userSession;

        public Container Container
        {
            get { return _container ?? (_container = AppHostBase.Instance.Container); }
        }

        protected string SessionKey
        {
            get
            {
                var sessionId = SessionFeature.GetSessionId();
                return sessionId == null ? null : SessionFeature.GetSessionKey(sessionId);
            }
        }

        protected AuthUserSession UserSession
        {
            get
            {
                if (_userSession != null) return _userSession;
                if (SessionKey != null)
                    _userSession = Cache.Get<AuthUserSession>(SessionKey);
                else
                    SessionFeature.CreateSessionIds();

                var unAuthorizedSession = new AuthUserSession();
                return _userSession ?? (_userSession = unAuthorizedSession);
            }
        }

        public new ICacheClient Cache
        {
            get { return Container.Resolve<ICacheClient>(); }
        }

        public ISessionFactory SessionFactory
        {
            get { return Container.Resolve<ISessionFactory>(); }
        }

        public new ISession Session
        {
            get { return _session ?? (_session = SessionFactory.GetOrCreateSession()); }
        }

        public void ClearSession()
        {
            _userSession = null;
            Cache.Remove(SessionKey);
        }
    }
}