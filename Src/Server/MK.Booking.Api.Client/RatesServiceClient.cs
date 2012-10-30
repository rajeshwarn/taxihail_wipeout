using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client
{
    public class RatesServiceClient : BaseServiceClient
    {
        public RatesServiceClient(string url, string sessionId) : base(url, sessionId)
        {
        }

        public void CreateRate(Rates rate)
        {
            var req = string.Format("/admin/rates");
            var response = Client.Post<string>(req, rate);
        }

        public void DeleteRate(Guid rateId)
        {
            var req = string.Format("/admin/rates/" + rateId);
            var response = Client.Delete<string>(req);
        }

        public IList<Rate> GetRates()
        {
            var req = string.Format("/admin/rates");
            return Client.Get<IList<Rate>>(req);
        }
    }
}