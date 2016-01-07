using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class IPAddressManager : IIPAddressManager
    {
        public string GetIPAddress()
        {
            try
            {
                string ipAddress = null;

                var adresses = Dns.GetHostAddresses(Dns.GetHostName());

                foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 
                        || netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                ipAddress = addrInfo.Address.ToString();
                            }
                        }
                    }
                }

                return ipAddress;
            }
            catch
            {
                return null;
            }
        }
    }
}
