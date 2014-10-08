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
        private readonly IServerSettings _configManager;

        public ApplicationInfoService(IServerSettings configManager)
        {
            _configManager = configManager;
        }


        public object Get(ApplicationInfoRequest request)
        {
            var info = new ApplicationInfo
            {
                Version = Assembly.GetAssembly(typeof (ApplicationInfoService)).GetName().Version.ToString(),
                SiteName = _configManager.ServerData.TaxiHail.SiteName
            };
            return info;
        }
    }
}