using System;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Linq;
using System.Reactive;
using System.Collections.Generic;
using System.Reactive.Disposables;
using MK.Common.iOS.Patterns;
using Cirrious.MvvmCross.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
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

            var position = new Position()
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

    
