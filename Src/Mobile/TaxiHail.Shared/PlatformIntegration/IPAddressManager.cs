using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Net;

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
    }
}