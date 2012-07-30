using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public class NearbyPlacesClient : BaseServiceClient
    {
        public NearbyPlacesClient(string url)
            : base(url)
        {
        }


        public Address[] GetNearbyPlaces(double? latitude, double? longitude, int? radius)
        {
            var result = Client.Get<Address[]>(string.Format("/places?lat={0}&lng={1}&radius={2}", latitude, longitude, radius));
            return result;
        }

        public Address[] GetNearbyPlaces(double? latitude, double? longitude)
        {
            var result = Client.Get<Address[]>(string.Format("/places?lat={0}&lng={1}", latitude, longitude));
            return result;
        }
    }
}
