using System;

using apcurium.Framework.Extensions;
namespace apcurium.MK.Booking.Mobile.Data
{
	public static class LocationDataExtension
	{
		
		public static bool HasValidCoordinate( this LocationData loc)
		{
		    var hasValideCoordinate = (loc != null) &&
		                              loc.Latitude.HasValue &&
		                              loc.Longitude.HasValue;
			//&& 		                              (!loc.Longitude.Value.AlmostEquals(0, 0.0001)) &&
		      //                        (!loc.Latitude.Value.AlmostEquals(0, 0.0001));


		    if (loc != null)
		    {
		        Console.WriteLine("HasValidCoordinate : " + loc.Address.ToSafeString() + " - " +
		                          hasValideCoordinate.ToString());
		    }

        	return hasValideCoordinate;

		}
		
	}
}

