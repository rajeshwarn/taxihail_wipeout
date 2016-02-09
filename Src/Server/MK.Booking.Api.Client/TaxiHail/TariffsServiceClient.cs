#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Common;


#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class TariffsServiceClient : BaseServiceClient
    {
        public TariffsServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService)
        {
        }

        public Task CreateTariff(Tariff tariff)
        {
            var req = string.Format("/admin/tariffs");
            return Client.PostAsync<string>(req, tariff);
        }

        public Task DeleteTariff(Guid tariffId)
        {
            var req = string.Format("/admin/tariffs/" + tariffId);
            return Client.DeleteAsync<string>(req);
        }

        public Task<IEnumerable<Tariff>> GetTariffs()
        {
            var req = string.Format("/admin/tariffs");
            return Client.GetAsync<IEnumerable<Tariff>>(req);
        }
    }
}