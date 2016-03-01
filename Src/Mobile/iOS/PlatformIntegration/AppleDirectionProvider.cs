using System;
using apcurium.MK.Booking.MapDataProvider;
using MapKit;
using CoreLocation;
using Foundation;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.MapDataProvider.Resources;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AppleDirectionProvider : IDirectionDataProvider
	{
		private readonly ILogger _logger;

		public AppleDirectionProvider (ILogger logger)
		{
			_logger = logger;
		}

        public Task<GeoDirection> GetDirectionsAsync (double originLat, double originLng, double destLat, double destLng, DateTime? date)
        {
            var tcs = new TaskCompletionSource<GeoDirection>();
            var result = new GeoDirection();

            var o = new NSObject ();
            o.InvokeOnMainThread (() => 
            {
                try 
                {
                    var origin = new CLLocationCoordinate2D (originLat, originLng);
                    var destination = new CLLocationCoordinate2D (destLat, destLng);

                    var emptyDict = new NSDictionary ();
                    var req = new MKDirectionsRequest 
                    {
                        Source = new MKMapItem (new MKPlacemark (origin, emptyDict)),
                        Destination = new MKMapItem (new MKPlacemark (destination, emptyDict)),
                        TransportType = MKDirectionsTransportType.Automobile,                        
                    };

                    if (date.HasValue) 
                    {
                        req.DepartureDate = DateTimeToNSDate (date.Value);
                    }
                                    
                    var dir = new MKDirections (req);
                    dir.CalculateDirections ((response, error) => 
                    {
                        if (error == null) 
                        {
                            var route = response.Routes [0];
                            result.Distance = Convert.ToInt32 (route.Distance);
                            result.Duration = Convert.ToInt32 (route.ExpectedTravelTime);
                        } 
                        else 
                        {
                            _logger.LogMessage("Error with CalculateDirections");
                            _logger.LogMessage("Error Code: " + error.Code);
                            _logger.LogMessage("Description: " + error.LocalizedDescription);
                        }

                        tcs.TrySetResult(result);
                    });
                } 
                catch (Exception ex) 
                {
                    _logger.LogMessage("Exception in AppleDirectionProvider");
                    _logger.LogError (ex);
                    tcs.TrySetResult(result);
                }
            });

            return tcs.Task;
        }

		private static NSDate DateTimeToNSDate (DateTime date)
		{
			return NSDate.FromTimeIntervalSinceReferenceDate ((date - (new DateTime (2001, 1, 1, 0, 0, 0))).TotalSeconds);
		}
	}
}

