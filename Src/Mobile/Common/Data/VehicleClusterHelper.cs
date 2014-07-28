using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Drawing;
using System;

namespace apcurium.MK.Booking.Mobile.Data
{
	public static class VehicleClusterHelper
	{
		// Maximum number of vehicles in a cell before we start displaying a cluster
		private const int _cellThreshold = 1;

		public static AvailableVehicle[] Clusterize(AvailableVehicle[] vehicles, MapBounds mapBounds) 
		{
			var result = new List<AvailableVehicle>();

			if (vehicles != null && vehicles.Any ())
			{
				var radius = (float)mapBounds.LongitudeDelta / 20;

				var list = new List<AvailableVehicle>(vehicles);

				/* Loop until all markers have been compared. */
				while(list.Count > 0)
				{
					var cluster = new VehicleClusterBuilder ();
					var toRemove = new List<AvailableVehicle>();

					var marker = list.First();
					list.Remove (marker);

					/* Compare against all markers which are left. */
					foreach (var target in list) 
					{
						var distance = GetDistanceBetweenPoints (
							new PointF ((float)marker.Latitude, (float)marker.Longitude), 
							new PointF ((float)target.Latitude, (float)target.Longitude));

						/* If two markers are closer than given distance remove */
						/* target marker from array and add it to cluster.      */
						if (radius > distance) 
						{
							toRemove.Add(target);
							cluster.Add(target);
						}
					}

					foreach (var itemToRemove in toRemove)
					{
						list.Remove (itemToRemove);
					}

					/* If a marker has been added to cluster, add also the one  */
					/* we were comparing to									    */
					if (!cluster.IsEmpty) 
					{
						cluster.Add(marker);
						result.Add(cluster.Build());
					}
					else 
					{
						result.Add(marker);
					}
				}
			}

			return result.ToArray();
		}

		private static double GetDistanceBetweenPoints(PointF point1, PointF point2)
		{
			return Math.Sqrt (Math.Pow (point2.X - point1.X, 2) + Math.Pow (point2.Y - point1.Y, 2));
		}
	}
}

