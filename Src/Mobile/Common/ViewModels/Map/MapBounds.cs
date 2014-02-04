using System;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Disposables;

namespace apcurium.MK.Booking.Mobile.ViewModels
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
	}
}

