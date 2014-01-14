using System;
using System.Collections.Generic;
using Android.Locations;
using Android.OS;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MK.Common.iOS.Patterns;
using Object = Java.Lang.Object;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    public class LocationListener : Object, ILocationListener, IObservable<Position>
    {
        private readonly List<IObserver<Position>> _observers;

        public LocationListener()
        {
            _observers = new List<IObserver<Position>>();
        }

        public Position LastKnownPosition { get; set; }
        public Position BestPosition { get; set; }

        public void OnLocationChanged(Location location)
        {
            var position = new Position
            {
                Time = location.Time.ToDateTime(),
                Error = location.Accuracy,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };

            foreach (var observer in _observers.ToArray())
            {
                observer.OnNext(position);
            }

            if (!BestPosition.IsBetterThan(position))
            {
                BestPosition = position;
            }

            LastKnownPosition = position;
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


        public IDisposable Subscribe(IObserver<Position> observer)
        {
            _observers.Add(observer);
            return new ActionDisposable(() => { _observers.Remove(observer); });
        }
    }
}