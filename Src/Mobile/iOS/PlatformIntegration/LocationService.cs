using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.CoreLocation;
using System.Threading.Tasks;
using System.Threading;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Diagnostics;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class LocationService : ILocationService
    {
        private CLLocation _lastValidLocation;
        private CLLocationManager _locationManager;
        private LocationManagerDelegate _locationDelegate;
        
        public LocationService()
        {
        }
        
        public Position LastKnownPosition
        {
            get
            { 
                if ((_locationDelegate != null) && (_locationDelegate.LastKnownLocation != null))
                {
                    return new Position{ 
                        Time = _locationDelegate.LastKnownLocation.Timestamp, 
                        Latitude =  _locationDelegate.LastKnownLocation.Coordinate.Latitude, 
                        Longitude = _locationDelegate.LastKnownLocation.Coordinate.Longitude,
                        Accuracy = (float)_locationDelegate.LastKnownLocation.HorizontalAccuracy,
                        Altitude = _locationDelegate.LastKnownLocation.Altitude,
                        Speed = (float)_locationDelegate.LastKnownLocation.Speed,
                    };
                }
                else
                {
                    return new Position{ Longitude = 98, Latitude = 40 };
                }
            }
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
                    if (result.HorizontalAccuracy <= accuracy) 
                    {
                        TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Good location found! : " +
                                                                               result.HorizontalAccuracy.ToString());
                        timeoutExpiredResult = false;
                        exit = true;
                    }
                }
                

                NSRunLoop.Current.RunUntil(DateTime.Now.AddMilliseconds(400));             
                
                if (watch.ElapsedMilliseconds >= timeout)
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
            if (_locationManager != null)
            {
                _locationManager.StartUpdatingLocation();
                return;
            }
            
            _locationManager = new CLLocationManager();
            _locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            _locationManager.DistanceFilter = -1;
            _locationDelegate = new LocationManagerDelegate();
            _locationManager.Delegate = _locationDelegate;
            _locationManager.StartUpdatingLocation();


        }
        
        private Task<Position> _last;


        public bool IsServiceEnabled
        {
            get { return CLLocationManager.Status == CLAuthorizationStatus.Authorized && CLLocationManager.LocationServicesEnabled ;}
        }
        public Task<Position> GetPositionAsync(int timeout, float accuracy, int fallbackTimeout, float fallbackAccuracy, CancellationToken cancelToken)
        {
           
            if ( ( _last != null ) && ( _last.Status == TaskStatus.Running  ))
            {
                return _last;
            }

            if (!IsServiceEnabled)
            {
                return new Task<Position>(() =>{ throw new Exception("Location service not enabled");} ); 
            }


            Initialize();

            Console.WriteLine ( "**********GetPositionAsync**********" );

            _last = new Task<Position>(() =>
                                       {
                 
                bool timedout = false;
                var result = WaitForAccurateLocation(timeout, accuracy, out timedout);

                if(timedout)
                {
                    result = WaitForAccurateLocation(fallbackTimeout, fallbackAccuracy, out timedout);
                    if ( timedout )
                    {
                        TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Location search timed out");
                        if ( _locationDelegate.LastKnownLocation != null )
                        {
                            result = _locationDelegate.LastKnownLocation;
                        }
                        else if ( _lastValidLocation != null )
                        {
                            result = _lastValidLocation;
                        }                         
                    }
                }
                return new Position { Latitude = result.Coordinate.Latitude, Longitude = result.Coordinate.Longitude };
            }, cancelToken);           

            _last.ContinueWith(t => {
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted );
            _last.Start();
            return _last;
            
        }
        

        public void Stop ()
        {
            _locationManager.StopUpdatingLocation ();
        }       
#endregion
    }
}
