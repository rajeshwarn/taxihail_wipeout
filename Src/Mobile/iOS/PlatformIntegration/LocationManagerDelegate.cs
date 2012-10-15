using System;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class LocationManagerDelegate : CLLocationManagerDelegate
    {


        public LocationManagerDelegate()
        {
        }

        public CLLocation LastKnownLocation
        {
            get;
            private set;
        }


        public static DateTime NSDateToDateTime (MonoTouch.Foundation.NSDate date)
        {
            return (new DateTime (2001, 1, 1, 0, 0, 0)).AddSeconds (date.SecondsSinceReferenceDate);
        }

        public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
        {
            double secondHowRecent = newLocation.Timestamp.SecondsSinceReferenceDate - NSDate.Now.SecondsSinceReferenceDate;
            
            if (( LastKnownLocation == null ) ||
                ((LastKnownLocation.Coordinate.Latitude == 0) || (LastKnownLocation.Coordinate.Longitude == 0) ||
                 (LastKnownLocation.HorizontalAccuracy > newLocation.HorizontalAccuracy)) && (secondHowRecent < -0.0 && secondHowRecent > -10.0))
            {
                LastKnownLocation = newLocation;
                Console.WriteLine ("********************UPDATING LOCATION**************************");
                Console.WriteLine ("Lat : " + newLocation.Coordinate.Latitude.ToString ());
                Console.WriteLine ("Long : " + newLocation.Coordinate.Longitude.ToString ());
                Console.WriteLine ("HAcc : " + newLocation.HorizontalAccuracy.ToString ());
                Console.WriteLine ("VAcc : " + newLocation.VerticalAccuracy.ToString ());
            }
            
        }

    }
}

    