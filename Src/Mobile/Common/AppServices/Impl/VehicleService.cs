using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		
		public Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude)
        {
			return UseServiceClient<IVehicleClient, AvailableVehicle[]>(service => service
				.GetAvailableVehiclesAsync( latitude, longitude));
        }
    }
}