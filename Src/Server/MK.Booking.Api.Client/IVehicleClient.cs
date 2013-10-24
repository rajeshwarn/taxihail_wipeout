using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile
{
	public interface IVehicleClient
	{				
	    Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude);
	}
}

	