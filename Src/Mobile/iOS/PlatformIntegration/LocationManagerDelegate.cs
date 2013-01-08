using System;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

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
             set;
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



                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("********************UPDATING LOCATION**************************");
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Lat : " + newLocation.Coordinate.Latitude.ToString ());
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Long : " + newLocation.Coordinate.Longitude.ToString ());
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("HAcc : " + newLocation.HorizontalAccuracy.ToString ());
            }
            
        }

    }
}

    