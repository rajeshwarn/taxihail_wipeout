using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Data;
using TinyIoC;
using Xamarin.Geolocation;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class UserPositionService : IUserPositionService
    {
        private Geolocator _fineGeolocator;
        private Geolocator _ballParkGeolocator;
        private Geolocator _coarseGeolocator;
        private Geolocator _mediumGeolocator;

        private Coordinate _lastKnownPosition;
        //private CancellationToken

        private string _timestamp;

        public UserPositionService()
        {
            _fineGeolocator = TinyIoCContainer.Current.Resolve<Geolocator>();
            _ballParkGeolocator = TinyIoCContainer.Current.Resolve<Geolocator>(CoordinatePrecision.BallPark.ToString());
            _coarseGeolocator = TinyIoCContainer.Current.Resolve<Geolocator>(CoordinatePrecision.Coarse.ToString());
            _mediumGeolocator = TinyIoCContainer.Current.Resolve<Geolocator>(CoordinatePrecision.Medium.ToString());
        }

        public void Refresh()
        {
            _timestamp = Guid.NewGuid().ToString();
            _ballParkGeolocator.GetPositionAsync(30000).ContinueWith(p => UpdateLocation(p, CoordinatePrecision.BallPark));
            _coarseGeolocator.GetPositionAsync(30000).ContinueWith(p => UpdateLocation(p, CoordinatePrecision.Coarse));
            _mediumGeolocator.GetPositionAsync(30000).ContinueWith(p => UpdateLocation(p, CoordinatePrecision.Medium));
            _fineGeolocator.GetPositionAsync(30000).ContinueWith(p => UpdateLocation(p, CoordinatePrecision.Fine));
        }


        private void UpdateLocation(Task<Position> p, CoordinatePrecision coordinatePrecision)
        {
            if (p.IsCompleted)
            {
                if (LastKnownPosition.RefreshTime == CoordinateRefreshTime.ALongTimeAgo)
                {
                    LastKnownPosition = new Coordinate { Latitude = p.Result.Latitude, Longitude = p.Result.Longitude, RefreshTimeInTicks = DateTime.Now.Ticks, Accuracy = p.Result.Accuracy };
                }
                else if ((LastKnownPosition.RefreshTime == CoordinateRefreshTime.NotRecently) &&
                                        ( (coordinatePrecision == CoordinatePrecision.Medium ) || 
                                          (coordinatePrecision == CoordinatePrecision.Fine )  ) )
                {
                    LastKnownPosition = new Coordinate { Latitude = p.Result.Latitude, Longitude = p.Result.Longitude, RefreshTimeInTicks = DateTime.Now.Ticks, Accuracy = p.Result.Accuracy };
                }
                else if ((LastKnownPosition.RefreshTime == CoordinateRefreshTime.Recently ) &&  (coordinatePrecision == CoordinatePrecision.Fine))
                {
                    LastKnownPosition = new Coordinate { Latitude = p.Result.Latitude, Longitude = p.Result.Longitude, RefreshTimeInTicks = DateTime.Now.Ticks, Accuracy = p.Result.Accuracy };
                }
                
                //Console.WriteLine("LOCATION UPDATED " + p.Result.Latitude.ToString() + ", " + p.Result.Longitude.ToString() + " : " + coordinatePrecision.ToString() + " ACCC " + p.Result.Accuracy.ToString());
            }
        }

        public Coordinate LastKnownPosition
        {
            get
            {
                if (_lastKnownPosition == null)
                {
                    // 44, -94
                    _lastKnownPosition = TinyIoCContainer.Current.Resolve<ICacheService>().Get<Coordinate>("LastKnownPosition");

                    if (_lastKnownPosition == null)
                    {
                        _lastKnownPosition = new Coordinate { Latitude = 44, Longitude = -94, Accuracy  = double.MaxValue , RefreshTimeInTicks = 0 };
                    }                    
                }
                return _lastKnownPosition;
            }
            private set
            {
                _lastKnownPosition = value;
                TinyIoCContainer.Current.Resolve<ICacheService>().Set<Coordinate>("LastKnownPosition", value );
            }

        }

    }
}