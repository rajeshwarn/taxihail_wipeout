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
	}
}

