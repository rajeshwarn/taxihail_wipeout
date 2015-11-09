#region

using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PopularAddressesServiceClient : BaseServiceClient
    {
        public PopularAddressesServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }


        public Task<IEnumerable<Address>> GetPopularAddresses()
        {
            return Client.GetAsync<IEnumerable<Address>>("/popularaddresses");
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