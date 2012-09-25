using System;
using MonoTouch.CoreLocation;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class AddressExtension
	{
	
		
	
		public static CLLocationCoordinate2D GetCoordinate( this Address instance )
        { 
            CLLocationCoordinate2D result = new CLLocationCoordinate2D(0,0);			
            if ( instance.Longitude!=0 && instance.Latitude !=0)
            {
                result = new CLLocationCoordinate2D( instance.Latitude , instance.Longitude );
            }				
            return result;
        }
	}
}

