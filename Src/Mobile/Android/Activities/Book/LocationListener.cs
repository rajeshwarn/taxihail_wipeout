using System;
using System.Collections.Generic;
using Android.Locations;
using Android.OS;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MK.Common.iOS.Patterns;
using Object = Java.Lang.Object;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    public class LocationListener : Object, ILocationListener
    {
        public LocationListener( LocationListenerManager manager )
        {
            Manager = manager;
        }

        protected LocationListenerManager Manager
        {
            get;
            set;
        }

        #region ILocationListener implementation

        public void OnLocationChanged(Location location)
        {
            Manager.OnLocationChanged(location);
        }

        public void OnProviderDisabled(string provider)
        {
            Manager.OnProviderDisabled(provider);
        }

        public void OnProviderEnabled(string provider)
        {
            Manager.OnProviderEnabled(provider);
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            Manager.OnStatusChanged( provider,  status,  extras);
        }

        #endregion

       


    }

    public class LocationListenerManager : Object, IObservable<Position>
    {
        private readonly List<IObserver<Position>> _observers;

        public LocationListenerManager()
        {
            _observers = new List<IObserver<Position>>();
            GpsListener = new LocationListener(this);
            NetworkListener = new LocationListener(this);


        }

        public LocationListener GpsListener { get; private set; }
        public LocationListener NetworkListener { get; private set; }

        public Position LastKnownPosition { get; set; }
        public Position BestPosition { get; set; }

        public void OnLocationChanged(Location location)
        {
            try
            {
                var position = new Position()
                {
                    Time = location.Time.ToDateTime(),
                    Error = location.Accuracy,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                };

                foreach (var observer in _observers.ToList())
                {
                    observer.OnNext(position);
                }

                if (!BestPosition.IsBetterThan(position))
                {
                    BestPosition = position;
                }

                LastKnownPosition = position;
            }
            catch 
            {
                //hack : crash randomly
            }
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