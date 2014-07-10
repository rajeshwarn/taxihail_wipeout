using System;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.MapDataProvider.TomTom.Resources;

namespace apcurium.MK.Booking.MapDataProvider.TomTom
{
    /// <summary>
    /// TomTom provider.
    /// documentation : http://developer.tomtom.com/docs/read/map_toolkit/web_services/routing/Request
    /// </summary>
    public class TomTomProvider : IDirectionDataProvider
    {
        private readonly IAppSettings _settings;
        private readonly ILogger _logger;

        private const string ApiUrl = "https://api.tomtom.com/lbs/services/";
        private const string RoutingServiceUrl = "route/3/{1}/Quickest/json?key={0}&avoidTraffic=true&includeTraffic=true&includeInstructions=false";
        private const string PointsFormat = "{0},{1}:{2},{3}";

        public TomTomProvider(IAppSettings settings, ILogger logger)
        {
            _logger = logger;
            _settings = settings;
        }

        protected string MapToolkitKey
        {
            get { return _settings.Data.TomTomMapToolkitKey; }
        }

        public GeoDirection GetDirections (double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude)
        {
            var client = new JsonServiceClient (ApiUrl);
            var queryString = string.Format (RoutingServiceUrl, 
                           MapToolkitKey, 
                           GetFormattedPoints (originLatitude, originLongitude, destinationLatitude, destinationLongitude));

            try
            {
                var direction = client.Get<RoutingResponse>(queryString);
                return new GeoDirection
                { 
                    Distance = direction.Route.Summary.TotalDistanceMeters,
                    Duration = direction.Route.Summary.TotalTimeSeconds,
                    TrafficDelay = direction.Route.Summary.TotalDelaySeconds
                };
            }
            catch(Exception e)
            {
                _logger.LogError (e);
                return new GeoDirection();
            }
        }

        private string GetFormattedPoints(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude)
        {
            return string.Format (PointsFormat, originLatitude, originLongitude, destinationLatitude, destinationLongitude);
        }
    }
}

