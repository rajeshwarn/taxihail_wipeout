using System.Threading.Tasks;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;


namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IVehicleService
	{
		Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude);
	}
}

