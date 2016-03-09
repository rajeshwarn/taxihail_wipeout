#region

using System.Web.UI;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;
using apcurium.MK.Web.App_Start;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Web
{
    public class PageBase : Page
    {
        private IUnityContainer _container;
        private SessionEntity _userSession;

        public IUnityContainer Container
        {
            get { return _container ?? (_container = UnityConfig.GetConfiguredContainer()); }
        }

        protected string SessionKey
        {
            get
            {
                var sessionId = Request.Cookies.Get("ss-pid").SelectOrDefault(cookie => cookie.Value);

                return "urn:iauthsession:{0}".InvariantCultureFormat(sessionId);
            }
        }

        protected SessionEntity UserSession
        {
            get
            {
                if (_userSession != null)
                {
                    return _userSession;
                }

                _userSession = SessionKey != null 
                    ? Cache.Get<SessionEntity>(SessionKey) 
                    : new SessionEntity();

                return _userSession;
            }
        }

        public new ICacheClient Cache
        {
            get { return Container.Resolve<ICacheClient>(); }
        }

        public void ClearSession()
        {
            _userSession = null;
            Cache.Remove(SessionKey);
        }
    }
}