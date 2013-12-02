using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile
{
	public interface IIbsFareClient
	{
        DirectionInfo GetDirectionInfoFromIbs(double pickupLatitude, double pickupLongitude, double dropoffLatitude, double dropofLongitude);
	}
}

	