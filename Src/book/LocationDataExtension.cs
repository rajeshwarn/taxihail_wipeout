using MonoTouch.CoreLocation;

namespace TaxiMobileApp
{
	public static class LocationDataExtension
	{
	
		
	
		public static CLLocationCoordinate2D GetCoordinate( this LocationData instance )
        { 
            CLLocationCoordinate2D result = new CLLocationCoordinate2D(0,0);			
            if ( instance.Longitude.HasValue && instance.Latitude.HasValue )
            {
                result = new CLLocationCoordinate2D( instance.Latitude.Value , instance.Longitude.Value );
            }				
            return result;
        }
	}
}

