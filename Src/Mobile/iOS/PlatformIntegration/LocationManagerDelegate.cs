using System;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class LocationManagerDelegate : CLLocationManagerDelegate
    {


        public LocationManagerDelegate ()
        {
        }

        public CLLocation LastKnownLocation {
            get;
            set;
        }

        public static DateTime NSDateToDateTime (MonoTouch.Foundation.NSDate date)
        {
            return (new DateTime (2001, 1, 1, 0, 0, 0)).AddSeconds (date.SecondsSinceReferenceDate);
        }

        public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
        {
        //    TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage ("************************** Raw GPS update : Lat {0},  Long {1}, Acc : {2}", newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude, newLocation.HorizontalAccuracy);

            double secondHowRecent = double.MaxValue;
            if ( LastKnownLocation != null )
            {
                secondHowRecent = NSDate.Now.SecondsSinceReferenceDate  - LastKnownLocation.Timestamp.SecondsSinceReferenceDate ;
            }
            if ( (LastKnownLocation == null) ||
                (LastKnownLocation.HorizontalAccuracy > newLocation.HorizontalAccuracy) || (secondHowRecent > 10   ))
            {
                LastKnownLocation = newLocation;
          //      TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage ("************************** Better GPS update : Lat {0},  Long {1}, Acc : {2}", newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude, newLocation.HorizontalAccuracy);
            }
            
        }

    }
}

    