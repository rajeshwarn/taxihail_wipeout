using Android.Locations;
using Android.OS;
using Java.Lang;

namespace TaxiMobile.Activities.Book
{
    public class LocationListener : Object, ILocationListener
    {        
        private LocationService _locationService;

        public LocationListener(LocationService locationService )
        {
            _locationService = locationService;            
        }

        public void OnLocationChanged(Android.Locations.Location location)
        {         
            _locationService.LocationChanged( location );
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }

       
    }
}