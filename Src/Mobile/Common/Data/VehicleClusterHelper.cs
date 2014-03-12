using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Data
{
	public static class VehicleClusterHelper
	{
		public static AvailableVehicle[] Clusterize(AvailableVehicle[] vehicles, MapBounds mapBounds)
		{
			var result = new List<AvailableVehicle>();
			if (vehicles != null && vehicles.Any())
			{
				// Divide the map in 25 cells (5*5)
				const int numberOfRows = 5;
				const int numberOfColumns = 5;
				// Maximum number of vehicles in a cell before we start displaying a cluster
				const int cellThreshold = 1;

				var clusterWidth = (float)mapBounds.LongitudeDelta / numberOfColumns;
				var clusterHeight = (float)mapBounds.LatitudeDelta / numberOfRows;

				var list = new List<AvailableVehicle>(vehicles);

				for (var rowIndex = 0; rowIndex < numberOfRows; rowIndex++)
				{
					for (var colIndex = 0; colIndex < numberOfColumns; colIndex++)
					{
						var bounds = new MapBounds()
						{
							NorthBound = mapBounds.NorthBound + clusterHeight * rowIndex,
							SouthBound = mapBounds.NorthBound - clusterHeight * (rowIndex + 1),
							WestBound = mapBounds.WestBound + clusterWidth * colIndex,
							EastBound = mapBounds.WestBound + clusterWidth * (colIndex + 1)
						};

						var vehiclesInRect = list.Where(v => bounds.Contains(v.Latitude, v.Longitude)).ToArray();
						if (vehiclesInRect.Length > cellThreshold)
						{
							var clusterBuilder = new VehicleClusterBuilder();
							foreach (var v in vehiclesInRect)
								clusterBuilder.Add(v);
							result.Add(clusterBuilder.Build());
						}
						else
						{
							result.AddRange(vehiclesInRect);
						}

						foreach (var v in vehiclesInRect)
						{
							list.Remove(v);
						}
					}
				}
			}
			return result.ToArray();
		}
	}
}

