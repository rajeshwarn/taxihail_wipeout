using System;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IGoogleService
	{
		Address[] GetNearbyPlaces(double? latitude, double? longitude, int? radius);
		Address[] GetNearbyPlaces(double? latitude, double? longitude);
	}
}

