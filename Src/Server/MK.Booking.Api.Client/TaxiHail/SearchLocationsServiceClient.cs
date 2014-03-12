#region

using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class SearchLocationsServiceClient : BaseServiceClient
    {
        public SearchLocationsServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }


        public async Task<Address[]> Search(string name, double latitude, double longitude)
        {
            var resource = string.Format(CultureInfo.InvariantCulture, "/searchlocation?Name={0}&Lat={1}&Lng={2}", name,
                latitude, longitude);

            var result = await Client.PostAsync<Address[]>(resource, new object());
            return result;
        }
    }
}