using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.CoreLocation;
using System.Threading.Tasks;
using System.Threading;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Diagnostics;
using MonoTouch.Foundation;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class LocationService : ILocationService
    {
        private CLLocationManager _locationManager;
        private LocationManagerDelegate _locationDelegate;
        private static ILogger LoggerService {get{ return TinyIoCContainer.Current.Resolve<ILogger>();}}
                
        public bool IsLocationServicesEnabled
        {
            get { return CLLocationManager.Status == CLAuthorizationStatus.Authorized && CLLocationManager.LocationServicesEnabled ;}
        }

        public LocationService()
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
            Positions = _locationDelegate;
        }
                
        public void Start()
        {   
            _locationManager.StartUpdatingLocation();
        }
        
        public void Stop ()
        {
            _locationManager.StopUpdatingLocation ();
        }
        
        public Position LastKnownPosition
        {
            get { return _locationDelegate.LastKnownPosition ?? new Position{ Longitude = 98, Latitude = 40 }; }
        }
                
        public IObservable<Position> Positions { get; private set; }

        
        public IObservable<Position> GetNextPosition(TimeSpan timeout, float maxAccuracy){

            return Positions.Where(t=>
            {
                return t.Accuracy <= maxAccuracy;
            })
                .Timeout(timeout)
                .Take(1);
        }
        /*
                
        public Position WaitForAccurateLocation(int timeout, float accuracy, out bool timeoutExpired)
        {
            
            var result = _locationDelegate.LastKnownPosition;
            
            LoggerService.LogMessage("Start WaitForAccurateLocation");
            
            timeoutExpired = true;
            
            bool exit = false;
            var timeoutExpiredResult = true;
            
            Thread.Sleep(200);
            
            var watch = new Stopwatch();
            watch.Start();            
            while (!exit)
            {
                if (_locationDelegate.LastKnownPosition != null)
                {                   
                    result = _locationDelegate.LastKnownPosition;
                    if (result.Accuracy <= accuracy) 
                    {
                        LoggerService.LogMessage("Good location found! : " + result.Accuracy.ToString());
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
                result = _locationDelegate.LastKnownPosition;
            }
            
            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Done WaitForAccurateLocation");
            
            timeoutExpired = timeoutExpiredResult;
            return result;
        }

        
        private Task<Position> _last;


        public Task<Position> GetPositionAsync(int timeout, float accuracy, int fallbackTimeout, float fallbackAccuracy, CancellationToken cancelToken)
        {
           
            if ( ( _last != null ) && ( _last.Status == TaskStatus.Running  ))
            {
                return _last;
            }

 if (!IsLocationServicesEnabled)
            {
                return new Task<Position>(() =>{ throw new Exception("Location service not enabled");} ); 
            }


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
                        if ( _locationDelegate.LastKnownPosition != null )
                        {
                            result = _locationDelegate.LastKnownPosition;
                        }                        
                    }
                }
                return new Position { Latitude = result.Latitude, Longitude = result.Longitude };
            }, cancelToken);           

            _last.ContinueWith(t => {
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted );
            _last.Start();
            return _last;
            
        }
        */

    }
}
