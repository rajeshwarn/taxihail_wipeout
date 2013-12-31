using apcurium.MK.Common.Entity;
using MonoTouch.CoreLocation;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
	public static class AddressExtension
	{
		public static CLLocationCoordinate2D GetCoordinate( this Address instance )
        { 
            var result = new CLLocationCoordinate2D(0,0);			
// ReSharper disable CompareOfFloatsByEqualityOperator
            if ( instance.Longitude!=0 && instance.Latitude !=0)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                result = new CLLocationCoordinate2D( instance.Latitude , instance.Longitude );
            }				
            return result;
        }
	}
}

