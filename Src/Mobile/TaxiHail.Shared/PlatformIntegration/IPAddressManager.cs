using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class IPAddressManager : IIPAddressManager
    {
        public string GetIPAddress()
        {
            try
            {
                var netInterfaceInfos = GetIPAddresses();

                foreach(var netInterfaceInfo in netInterfaceInfos)
                {
                    var ip = netInterfaceInfo.Value;
                    if (!string.IsNullOrEmpty(ip))
                    {
                        return ip;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public IDictionary<string,string> GetIPAddresses()
        {
            var allAddresses = Dns.GetHostAddresses(Dns.GetHostName());

            var addressesDictionary = new Dictionary<string, string>();
            for (var i = 0; i < allAddresses.Length; i++)
            {
                addressesDictionary.Add(i.ToString(), allAddresses.Skip(i).Take(1).FirstOrDefault().ToString());
            }

            return addressesDictionary;
        }
    }
}