using System.Net;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{

    public interface IIPAddressManager
    {
        /// <summary>
        /// Returns the cellular network ip address if it exists, otherwise returns null.
        /// </summary>
        /// <returns>The IP address.</returns>
        string GetIPAddress();

        IDictionary<string,IPAddress> GetIPAddresses();
    }
}
