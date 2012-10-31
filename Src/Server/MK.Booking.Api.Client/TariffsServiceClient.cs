using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Client
{
    public class TariffsServiceClient : BaseServiceClient
    {
        public TariffsServiceClient(string url, string sessionId) : base(url, sessionId)
        {
        }

        public void CreateTariff(Tariff tariff)
        {
            var req = string.Format("/admin/tariffs");
            var response = Client.Post<string>(req, tariff);
        }

        public void DeleteTariff(Guid tariffId)
        {
            var req = string.Format("/admin/tariffs/" + tariffId);
            var response = Client.Delete<string>(req);
        }

        public IList<Tariff> GetTariffs()
        {
            var req = string.Format("/admin/tariffs");
            return Client.Get<IList<Tariff>>(req);
        }
    }
}