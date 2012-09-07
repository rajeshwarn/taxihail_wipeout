using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Google.Resources;

namespace apcurium.MK.Booking.Google
{
    public interface IMapsApiClient
    {
        Place[] GetNearbyPlaces(double? latitude, double? longitude,string name, string languageCode, bool sensor, int radius);        
        DirectionResult GetDirections(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude);
        GeoResult GeocodeAddress(string address);
        GeoResult GeocodeLocation(double latitude, double longitude);
    }
}
