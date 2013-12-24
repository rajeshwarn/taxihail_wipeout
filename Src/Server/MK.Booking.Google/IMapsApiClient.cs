#region

using apcurium.MK.Booking.Google.Resources;

#endregion

namespace apcurium.MK.Booking.Google
{
    public interface IMapsApiClient
    {
        Place[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, bool sensor, int radius,
            string pipedTypeList = null);

        Place[] SearchPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor,
            int radius, string countryCode);

        DirectionResult GetDirections(double originLatitude, double originLongitude, double destinationLatitude,
            double destinationLongitude);

        GeoResult GeocodeAddress(string address);
        GeoResult GeocodeLocation(double latitude, double longitude);
        GeoObj GetPlaceDetail(string reference);
    }
}