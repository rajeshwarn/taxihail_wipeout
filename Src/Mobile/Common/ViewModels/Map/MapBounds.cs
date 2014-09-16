using System;

namespace apcurium.MK.Booking.Maps.Geo
{
	public class MapBounds
	{
		public double NorthBound { get; set; }
		public double WestBound { get; set; }
		public double SouthBound { get; set; }
		public double EastBound { get; set; }

		public double LatitudeDelta
		{
			get { return Math.Abs(NorthBound - SouthBound);  }
		}

		public double LongitudeDelta
		{
			get { return Math.Abs(WestBound - EastBound);  }
		}

		public Coordinate GetCenter()
		{
			return new Coordinate
			{
				Latitude = (NorthBound + SouthBound) / 2,
				Longitude =  (EastBound + WestBound) / 2,
			};
		}

		public class Coordinate
		{
			public double Latitude { get; set; }
			public double Longitude { get; set; }
		}



		public bool Contains(double latitude, double longitude)
		{
			if (NorthBound >= latitude && latitude >= SouthBound)
			{
				if(WestBound <= EastBound && WestBound <= longitude && longitude <= EastBound)
				{
					return true;
				}
				else 
				{
					if (WestBound > EastBound && (WestBound <= longitude || longitude <= EastBound))
					{
						return true;
					}
				}
			}
			return false;
		}


		const double ASSUMED_INIT_LATLNG_DIFF = 1.0;
		const double ACCURACY = 0.01d;

		// Ported from Java - http://stackoverflow.com/a/16417396/787856
		public static MapBounds GetBoundsFromCenterAndRadius(double centerLatitude, double centerLongitude, double latRadiusInMeters, double lngRadiusInMeters) 
		{

			latRadiusInMeters /= 2;
			lngRadiusInMeters /= 2;

			Position north, south, east, west;

			double distance = 0f;

			{
				bool foundMax = false;
				double foundMinLngDiff = 0;
				double assumedLngDiff = ASSUMED_INIT_LATLNG_DIFF;

				do {

					distance = Position.CalculateDistance(centerLatitude, centerLongitude, centerLatitude, centerLongitude + assumedLngDiff);

					double distanceDiff = distance - lngRadiusInMeters;
					if (distanceDiff < 0) {
						if (!foundMax) {
							foundMinLngDiff = assumedLngDiff;
							assumedLngDiff *= 2;
						} else {
							double tmp = assumedLngDiff;
							assumedLngDiff += (assumedLngDiff - foundMinLngDiff) / 2;
							foundMinLngDiff = tmp;
						}
					} else {
						assumedLngDiff -= (assumedLngDiff - foundMinLngDiff) / 2;
						foundMax = true;
					}
				} while (Math.Abs(distance - lngRadiusInMeters) > lngRadiusInMeters * ACCURACY);

				east = new Position(centerLatitude, centerLongitude + assumedLngDiff);
				west = new Position(centerLatitude, centerLongitude - assumedLngDiff);		
			}

			{
				bool foundMax = false;
				double foundMinLatDiff = 0;
				double assumedLatDiffNorth = ASSUMED_INIT_LATLNG_DIFF;

				do {

					distance = Position.CalculateDistance(centerLatitude, centerLongitude, centerLatitude + assumedLatDiffNorth, centerLongitude);

					double distanceDiff = distance - latRadiusInMeters;
					if (distanceDiff < 0) {
						if (!foundMax) {
							foundMinLatDiff = assumedLatDiffNorth;
							assumedLatDiffNorth *= 2;
						} else {
							double tmp = assumedLatDiffNorth;
							assumedLatDiffNorth += (assumedLatDiffNorth - foundMinLatDiff) / 2;
							foundMinLatDiff = tmp;
						}
					} else {
						assumedLatDiffNorth -= (assumedLatDiffNorth - foundMinLatDiff) / 2;
						foundMax = true;
					}

				} while (Math.Abs(distance - latRadiusInMeters) > latRadiusInMeters * ACCURACY);

				north = new Position(centerLatitude + assumedLatDiffNorth, centerLongitude);
			}

			{
				bool foundMax = false;
				double foundMinLatDiff = 0;
				double assumedLatDiffSouth = ASSUMED_INIT_LATLNG_DIFF;

				do {

					distance = Position.CalculateDistance(centerLatitude, centerLongitude, centerLatitude - assumedLatDiffSouth, centerLongitude);

					double distanceDiff = distance - latRadiusInMeters;
					if (distanceDiff < 0) {
						if (!foundMax) {
							foundMinLatDiff = assumedLatDiffSouth;
							assumedLatDiffSouth *= 2;
						} else {
							double tmp = assumedLatDiffSouth;
							assumedLatDiffSouth += (assumedLatDiffSouth - foundMinLatDiff) / 2;
							foundMinLatDiff = tmp;
						}
					} else 
					{
						assumedLatDiffSouth -= (assumedLatDiffSouth - foundMinLatDiff) / 2;
						foundMax = true;
					}

				} while (Math.Abs(distance - latRadiusInMeters) > latRadiusInMeters * ACCURACY);

				south = new Position(centerLatitude - assumedLatDiffSouth, centerLongitude);
			}

			return new MapBounds () { NorthBound = north.Latitude, SouthBound = south.Latitude, EastBound = east.Longitude, WestBound = west.Longitude };
		}
	}
}

