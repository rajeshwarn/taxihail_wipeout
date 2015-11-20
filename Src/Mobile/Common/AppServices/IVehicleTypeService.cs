using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;

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

