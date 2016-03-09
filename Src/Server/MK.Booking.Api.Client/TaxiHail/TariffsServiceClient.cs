#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class TariffsServiceClient : BaseServiceClient
    {
        public TariffsServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
        }

        public Task CreateTariff(Tariff tariff)
        {
            var req = string.Format("/admin/tariffs");
            return Client.PostAsync<string>(req, tariff, logger: Logger);
        }

        public Task DeleteTariff(Guid tariffId)
        {
            var req = string.Format("/admin/tariffs/" + tariffId);
            return Client.DeleteAsync<string>(req, logger: Logger);
        }

        public Task<IEnumerable<Tariff>> GetTariffs()
        {
            var req = string.Format("/admin/tariffs");
            return Client.GetAsync<IEnumerable<Tariff>>(req, logger: Logger);
        }
    }
}