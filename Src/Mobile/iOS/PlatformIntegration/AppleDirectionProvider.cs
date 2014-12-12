using System;
using System.Linq;
using apcurium.MK.Booking.MapDataProvider;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using System.Threading;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AppleDirectionProvider : IDirectionDataProvider
	{
		private readonly ILogger _logger;

		public AppleDirectionProvider (ILogger logger )
		{
			_logger = logger;
		}



		#region IDirectionDataProvider implementation

		public apcurium.MK.Booking.MapDataProvider.Resources.GeoDirection GetDirections (double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude)
		{

			try {

				var auto = new AutoResetEvent (false);

				var d = new apcurium.MK.Booking.MapDataProvider.Resources.GeoDirection ();

				var o = new NSObject ();
				o.InvokeOnMainThread (() => {
					var origin = new CLLocationCoordinate2D (originLatitude, originLongitude);
					var destination = new CLLocationCoordinate2D (destinationLatitude, destinationLongitude);

					var emptyDict = new NSDictionary ();
					var req = new MKDirectionsRequest () {
						Source = new MKMapItem (new MKPlacemark (origin, emptyDict)),
						Destination = new MKMapItem (new MKPlacemark (destination, emptyDict)),
						TransportType = MKDirectionsTransportType.Automobile,						 
					};


						
					var dir = new MKDirections (req);
					dir.CalculateDirections ((response, error) => { 
						if (error == null) {
							var route = response.Routes [0];
							d.Distance = System.Convert.ToInt32 (route.Distance);
							d.Duration = System.Convert.ToInt32 (route.ExpectedTravelTime);
							auto.Set ();

						} else {
							_logger.LogMessage( "Error with direction api" );
							_logger.LogMessage( error.Description );
							_logger.LogMessage( error.DebugDescription );

						}

					});
				});

				auto.WaitOne ();
				return d;
			} catch (Exception ex) {
				_logger.LogMessage( "Exception with direction api" );
				_logger.LogError (ex);
			}


			return null;
		}

		public static MonoTouch.Foundation.NSDate DateTimeToNSDate (DateTime date)
		{
			return MonoTouch.Foundation.NSDate.FromTimeIntervalSinceReferenceDate ((date - (new DateTime (2001, 1, 1, 0, 0, 0))).TotalSeconds);
		}


		#endregion
	}
}

