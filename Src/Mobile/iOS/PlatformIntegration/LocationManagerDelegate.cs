using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore.Touch;
using MK.Common.iOS.Patterns;
using MonoTouch.CoreLocation;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class LocationManagerDelegate : CLLocationManagerDelegate, IObservable<Position>
    {
        public LocationManagerDelegate ()
        {
            Observers = new List<IObserver<Position>>();
        }

        List<IObserver<Position>> Observers {get; set;}
        public Position LastKnownPosition {get;set;}
        public Position BestPosition {get;set;}

        [Obsolete ("Deprecated in iOS 6.0")]
        public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
        {
            var locations = new List<CLLocation> ();
            if (oldLocation != null) locations.Add (oldLocation);
            locations.Add (newLocation);
            LocationsUpdated (manager, locations.ToArray ());
        }

        public override void LocationsUpdated (CLLocationManager manager, CLLocation[] locations)
        {
            var newLocation = locations.Last();

            var position = new Position
            {
                Error = (float)newLocation.HorizontalAccuracy,
                Time = newLocation.Timestamp.ToDateTimeUtc(),
                Latitude = newLocation.Coordinate.Latitude,
                Longitude = newLocation.Coordinate.Longitude 
            };

            foreach(var observer in Observers.ToArray())
            {
                observer.OnNext(position);
            }

            if(!BestPosition.IsBetterThan(position))
            {
                BestPosition = position;
            }

            LastKnownPosition = position;
        }


        public IDisposable Subscribe (IObserver<Position> observer)
        {
            Observers.Add(observer);
            return new ActionDisposable(()=>{
                Observers.Remove(observer);
            });
        }
    }
}

    
