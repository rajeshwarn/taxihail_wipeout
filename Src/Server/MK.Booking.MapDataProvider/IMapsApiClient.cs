#region


using apcurium.MK.Booking.MapDataProvider.Resources;

#endregion

namespace apcurium.MK.Booking.MapDataProvider
{
    public interface IMapsApiClient
    {
        Place[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, bool sensor, int radius,
            string pipedTypeList = null);

        Place[] SearchPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor,
            int radius, string countryCode);

        DirectionResult GetDirections(double originLatitude, double originLongitude, double destinationLatitude,
            double destinationLongitude);

        GeoAddress[] GeocodeAddress(string address);
        GeoAddress[]  GeocodeLocation(double latitude, double longitude);
        GeoAddress GetPlaceDetail(string reference);
    }
}