﻿using System;
using apcurium.MK.Booking.MapDataProvider.Resources;
using MK.Booking.MapDataProvider.Foursquare;

namespace apcurium.MK.Booking.MapDataProvider
{
	public interface IPlaceDataProvider
	{
		GeoPlace[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, bool sensor, int radius, uint maximumNumberOfPlaces = 0, string pipedTypeList = null);

		GeoPlace[] SearchPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor, int radius, string countryCode);

		GeoPlace GetPlaceDetail (string id);
	}
}