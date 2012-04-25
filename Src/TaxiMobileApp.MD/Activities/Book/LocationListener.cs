using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

namespace TaxiMobile.Activities.Book
{
    public class LocationListener : Java.Lang.Object, ILocationListener
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