using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.CoreLocation;
using System.Threading.Tasks;
using System.Threading;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Diagnostics;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class LocationService : ILocationService
    {
        private CLLocationManager _locationManager;
        private LocationManagerDelegate _locationDelegate;

        public LocationService()
        {
        }

        public CLLocation WaitForAccurateLocation(int timeout, float accuracy, out bool timeoutExpired)
        {
           
            var result = _locationDelegate.LastKnownLocation;
            
            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Start WaitForAccurateLocation");
            
            timeoutExpired = true;
            
            bool exit = false;
            var timeoutExpiredResult = true;
            
            Thread.Sleep(200);
            
            var watch = new Stopwatch();
            watch.Start();            
            while (!exit)
            {
                if (_locationDelegate.LastKnownLocation != null)
                {                   
                    result = _locationDelegate.LastKnownLocation;
                    if ( (result.HorizontalAccuracy <= accuracy) || ( result.VerticalAccuracy <= accuracy ) )
                    {
                        TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Good location found! : " +
                                                                               result.HorizontalAccuracy.ToString());
                        timeoutExpiredResult = false;
                        exit = true;
                    }
                }
                
                
                Thread.Sleep(200);
                
                if ( watch.ElapsedMilliseconds >= timeout )
                {
                    exit = true;
                    timeoutExpiredResult = true;
                }
            }
            
            
            if (timeoutExpiredResult)
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Location search timed out");
                result = _locationDelegate.LastKnownLocation;
            }
            
            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Done WaitForAccurateLocation");
            
            timeoutExpired = timeoutExpiredResult;
            return result;
        }

        #region ILocationService implementation

        public void Initialize()
        {
            if ( _locationManager != null  )
            {
                return;
            }

            _locationManager = new CLLocationManager ();
            _locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            _locationManager.DistanceFilter = -1;
            _locationDelegate = new LocationManagerDelegate();
            _locationManager.Delegate = _locationDelegate;
            _locationManager.StartUpdatingLocation ();
        }

        public Task<Position> GetPositionAsync(int timeout, float accuracy, CancellationToken cancelToken)
        {
            Initialize();
            var task = new Task<Position>(() =>
                                          {
                bool timedout = false;
                var result = WaitForAccurateLocation(timeout, accuracy, out timedout);

                return new Position { Latitude = result.Coordinate.Latitude, Longitude = result.Coordinate.Longitude };
            }, cancelToken);
            
            task.Start();
            return task;

        }

        #endregion
    }
}

