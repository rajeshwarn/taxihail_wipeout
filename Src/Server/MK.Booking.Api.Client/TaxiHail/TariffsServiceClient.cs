#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class TariffsServiceClient : BaseServiceClient
    {
        public TariffsServiceClient(string url, string sessionId, string userAgent) : base(url, sessionId, userAgent)
        {
        }

        public void CreateTariff(Tariff tariff)
        {
            var req = string.Format("/admin/tariffs");
            Client.Post<string>(req, tariff);
        }

        public void DeleteTariff(Guid tariffId)
        {
            var req = string.Format("/admin/tariffs/" + tariffId);
            Client.Delete<string>(req);
        }

        public IList<Tariff> GetTariffs()
        {
            var req = string.Format("/admin/tariffs");
            return Client.Get<IList<Tariff>>(req);
        }
    }
}