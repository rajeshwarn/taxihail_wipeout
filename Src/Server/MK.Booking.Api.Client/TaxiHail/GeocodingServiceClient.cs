using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class GeocodingServiceClient : BaseServiceClient
    {
        public GeocodingServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }
        
        public Address[] Search(string addressName)
        {
            var result = Client.Post<Address[]>("/geocode", new
            {
                Name = addressName
            });
            return result;
        }

        public Task<Address[]> Search(double latitude, double longitude)
        {
            var resource = string.Format(CultureInfo.InvariantCulture, "/geocode?Lat={0}&Lng={1}", latitude, longitude);
            var result = Client.PostAsync<Address[]>(resource, new object());
            return result;
        }

        public Task<Address> DefaultLocation()
        {
            var result = Client.GetAsync<Address>("/settings/defaultlocation");
            return result;
        }
    }
}