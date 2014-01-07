using System.Collections.Generic;
using System.Collections;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleService : BaseService, IVehicleService
    {
		public VehicleService()
        {

        }
		
		public async Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude)
        {
			AvailableVehicle[] vehicles = null;

			UseServiceClient<IVehicleClient>(async service =>
				{
					vehicles = await service.GetAvailableVehiclesAsync( latitude, longitude);
				});

			return vehicles;
        }
    }
}