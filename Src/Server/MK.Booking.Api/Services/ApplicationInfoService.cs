#region

using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ApplicationInfoService : Service
    {
        private readonly IServerSettings _serverSettings;

        public ApplicationInfoService(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public object Get(ApplicationInfoRequest request)
        {
            var info = new ApplicationInfo
            {
                Version = Assembly.GetAssembly(typeof (ApplicationInfoService)).GetName().Version.ToString(),
                SiteName = _serverSettings.ServerData.TaxiHail.SiteName
            };
            return info;
        }
    }
}