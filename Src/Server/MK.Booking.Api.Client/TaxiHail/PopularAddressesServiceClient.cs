#region

using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
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


        public Task<IList<Address>> GetPopularAddresses()
        {
            var req = string.Format("/popularaddresses");
            var addresses = Client.GetAsync<IList<Address>>(req);
            return addresses;
        }

#if  !CLIENT
        public void Add(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses");
            Client.Post<object>(req, address);
        }
#endif
    }
}