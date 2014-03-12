#region

using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PopularAddressesServiceClient : BaseServiceClient
    {
        public PopularAddressesServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }


        public Task<IEnumerable<Address>> GetPopularAddresses()
        {
            var req = string.Format("/popularaddresses");
            var addresses = Client.GetAsync<IEnumerable<Address>>(req);
            return addresses;
        }

#if  !CLIENT
        public Task Add(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses");
            return Client.PostAsync<object>(req, address);
        }
#endif
    }
}