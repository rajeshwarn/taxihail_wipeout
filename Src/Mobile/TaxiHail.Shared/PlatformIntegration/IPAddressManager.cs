using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Net;
using System.Collections.Generic;
using System;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class IPAddressManager : IIPAddressManager
    {
        public string GetIPAddress()
        {
            try
            {
                var adresses = Dns.GetHostAddresses(Dns.GetHostName());

                if (adresses != null && adresses[0] != null)
                {
                    return adresses[0].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public IDictionary<string,IPAddress> GetIPAddresses()
        {
            throw new NotImplementedException();
        }
    }
}