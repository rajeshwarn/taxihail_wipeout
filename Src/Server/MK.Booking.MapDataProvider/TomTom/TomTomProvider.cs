﻿using System;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.MapDataProvider.TomTom.Resources;
using ServiceStack.Text;
using System.Globalization;
using System.Threading.Tasks;

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
		private const string RoutingServiceUrl = "route/3/{1}/Quickest/json?key={0}&avoidTraffic=true&includeTraffic=true&includeInstructions=false{2}";
		private const string PointsFormat = "{0},{1}:{2},{3}";
		private const string DateTimeFormat = "&day={0}&time={1}";

        public TomTomProvider(IAppSettings settings, ILogger logger)
		{
			_logger = logger;
			_settings = settings;
		}

		protected string MapToolkitKey
		{
			get { return _settings.Data.TomTomMapToolkitKey; }
		}

        public GeoDirection GetDirections(double originLat, double originLng, double destLat, double destLng, DateTime? date)
        {
            return GetDirectionsAsync(originLat, originLng, destLat, destLng, date).Result;
        }

        public Task<GeoDirection> GetDirectionsAsync (double originLat, double originLng, double destLat, double destLng, DateTime? date)
		{
            var tcs = new TaskCompletionSource<GeoDirection>();
            var result = new GeoDirection();

			var client = new JsonServiceClient (ApiUrl);
			var queryString = string.Format (CultureInfo.InvariantCulture, RoutingServiceUrl, 
				MapToolkitKey, 
                GetFormattedPoints (originLat, originLng, destLat, destLng),
				GetDayAndTimeParameter(date));

			_logger.LogMessage ("Calling TomTom : " + queryString);

			try
			{
				var direction = client.Get<RoutingResponse>(queryString);

				_logger.LogMessage ("TomTom Result : " + direction.ToJson());

                result.Distance = direction.Route.Summary.TotalDistanceMeters;
                result.Duration = direction.Route.Summary.TotalTimeSeconds;         // based on history for given day and time
                result.TrafficDelay = direction.Route.Summary.TotalDelaySeconds;    // this will only be available if date = null, otherwise it's 0
			}
			catch(Exception e)
			{
				_logger.LogError (e);
			}

            tcs.TrySetResult(result);

            return tcs.Task;
		}

		private string GetFormattedPoints(double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude)
		{
			return string.Format (CultureInfo.InvariantCulture, PointsFormat, originLatitude, originLongitude, destinationLatitude, destinationLongitude  );
		}

		private string GetDayAndTimeParameter(DateTime? date)
		{
			if (!date.HasValue)
			{
				return string.Empty;
			}

			// for which day? today, tomorrow, monday, tuesday, wednesday, thursday, friday, saturday, sunday
			string day = string.Empty;
			if (date.Value.Date == DateTime.Today)
			{
				day = "today";
			}
			else
			{
				if (date.Value.Date == DateTime.Today.AddDays (1))
				{
					day = "tomorrow";
				}
				else
				{
					day = date.Value.DayOfWeek.ToString ().ToLowerInvariant ();
				}
			}

			// when? either now or number of minutes since local midnight (between 0 and 1439)
			var time = (int)date.Value.TimeOfDay.TotalMinutes;

			return string.Format(DateTimeFormat, day, time);
		}
	}
}

