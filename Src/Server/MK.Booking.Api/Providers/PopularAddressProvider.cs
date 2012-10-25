using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Api.Providers
{
    public class PopularAddressProvider : IPopularAddressProvider
    {
        public IEnumerable<Address> GetPopularAddresses()
        {
            return Enumerable.Empty<Address>();
        }
    }
}
