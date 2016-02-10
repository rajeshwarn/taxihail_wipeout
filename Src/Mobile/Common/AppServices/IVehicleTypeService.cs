using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IVehicleTypeService
	{
		IObservable<IList<VehicleType>> GetAndObserveVehiclesList();

		void ClearVehicleTypesCache();
	}
}

