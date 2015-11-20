using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleTypeService : BaseService, IVehicleTypeService
	{
		private readonly ICacheService _cacheService;

		private const string VehicleTypesDataCacheKey = "VehicleTypesData";

		public VehicleTypeService(ICacheService cacheService)
		{
			this._cacheService = cacheService;
		}

		public async Task<IList<VehicleType>> GetVehiclesList()
		{
			var cached = _cacheService.Get<VehicleType[]>(VehicleTypesDataCacheKey);
			if (cached != null)
			{
				return cached;
			}

			var vehiclesList = await UseServiceClientAsync<IVehicleClient, VehicleType[]>(service => service.GetVehicleTypes());
			_cacheService.Set(VehicleTypesDataCacheKey, vehiclesList);

			return vehiclesList;
		}

		public async Task ResetLocalVehiclesList()
		{
			var vehiclesList = await UseServiceClientAsync<IVehicleClient, VehicleType[]>(service => service.GetVehicleTypes());
			_cacheService.Set(VehicleTypesDataCacheKey, vehiclesList);
		}

		public void SetMarketVehiclesList(List<VehicleType> marketVehicleTypes)
		{
			if (marketVehicleTypes.Any())
			{
				_cacheService.Set(VehicleTypesDataCacheKey, marketVehicleTypes);
			}
		}

		public void ClearVehicleTypesCache()
		{
			_cacheService.Clear(VehicleTypesDataCacheKey);
		}
	}
}

