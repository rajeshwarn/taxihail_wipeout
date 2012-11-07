using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client
{
    public class PopularAddressesServiceClient: BaseServiceClient
    {
        public PopularAddressesServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }


        public IList<Address> GetPopularAddresses()
        {
            var req = string.Format("/popularaddresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

#if  !CLIENT
        public void Add(PopularAddress address )
        {
            var req = string.Format("/admin/popularaddresses");
            Client.Post<object>(req, address);
        }
#endif
    }
}
