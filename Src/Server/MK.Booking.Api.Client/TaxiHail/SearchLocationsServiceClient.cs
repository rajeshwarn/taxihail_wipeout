#region

using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Common;


#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class SearchLocationsServiceClient : BaseServiceClient
    {
        public SearchLocationsServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService)
        {
        }


        public async Task<Address[]> Search(string name, double latitude, double longitude)
        {
            try
            {
                var resource = string.Format(CultureInfo.InvariantCulture, "/searchlocation?Name={0}&Lat={1}&Lng={2}", name,
                latitude, longitude);

                var result = await Client.PostAsync<Address[]>(resource, new object());
                return result;
            }
            catch (Exception ex)
            {
                
                throw;
            }
            
        }
    }
}