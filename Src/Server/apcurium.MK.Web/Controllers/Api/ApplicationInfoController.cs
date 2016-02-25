using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("app")]
    public class ApplicationInfoController : BaseApiController
    {
        private readonly IServerSettings _serverSettings;
        private readonly IAppStartUpLogDao _appStartUpLogDao;

        public ApplicationInfoController(IServerSettings serverSettings, IAppStartUpLogDao appStartUpLogDao)
        {
            _serverSettings = serverSettings;
            _appStartUpLogDao = appStartUpLogDao;
        }

        [HttpGet, Route("info")]
        public ApplicationInfo GetApplicationInfo()
        {
            if (_serverSettings.ServerData.DisableNewerVersionPopup)
            {
                throw new Exception("Newer version popup is disabled.");
            }

            return new ApplicationInfo
            {
                Version = Assembly.GetAssembly(typeof(ApplicationInfoController)).GetName().Version.ToString(),
                SiteName = _serverSettings.ServerData.TaxiHail.SiteName,
                MinimumRequiredAppVersion = _serverSettings.ServerData.MinimumRequiredAppVersion
            }; ;
        }

        [HttpGet, Route("starts/{lastMinutes}"), Auth(Roles = new []{Roles.Admin})]
        public AppStartUpLogDetail[] AppStartUpLog(long lastMinutes)
        {
            // the client sends a utc date, but when it's written in the database, it's converted to server's local time
            // when we get it back from entity framework, the "kind" is unspecified, which is converted to local
            return _appStartUpLogDao.GetAll()
                .Where(x => x.DateOccured >= DateTime.Now.AddMinutes(-lastMinutes))
                .ToArray();
        }
    }
}
