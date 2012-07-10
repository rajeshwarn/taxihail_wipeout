using System;
using TaxiMobileApp.Lib.GoogleServices;

namespace TaxiMobileApp.Lib.Data
{
	public class DataMapper
	{
		public static LocationData GooglePlaceDataToLocationData ( GooglePlaceData source )
		{
			var destination = new LocationData();
			if( source == null )
			{
				return destination;
			}

			destination.Address = source.vicinity;
			destination.IsAddNewItem = false;
			destination.IsFromHistory = false;
			destination.IsGPSDetected = true;
			destination.IsGPSNotAccurate = false;
			destination.IsHistoricEmptyItem = false;
			destination.IsNew = false;
			destination.Latitude = source.geometry.location.lat;
			destination.Longitude = source.geometry.location.lng;
			destination.Name = source.name;

			return destination;
		}
	}
}

