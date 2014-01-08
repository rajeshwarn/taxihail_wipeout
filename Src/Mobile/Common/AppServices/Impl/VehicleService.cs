using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		
		public async Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude)
        {
			AvailableVehicle[] vehicles = null;

            //todo potential bug here with the async in front of the lambda
			UseServiceClient<IVehicleClient>(async service =>
				{
					vehicles = await service.GetAvailableVehiclesAsync( latitude, longitude);
				});

			return vehicles;
        }
    }
}