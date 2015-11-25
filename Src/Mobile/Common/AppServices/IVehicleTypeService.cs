using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IVehicleTypeService
	{
		Task<IList<VehicleType>> GetVehiclesList();

		void SetMarketVehiclesList(List<VehicleType> marketVehicleTypes);

		Task ResetLocalVehiclesList();

		void ClearVehicleTypesCache();
	}
}

