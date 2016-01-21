using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client;
using System.Reactive.Subjects;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class VehicleTypeService : BaseService, IVehicleTypeService
	{
		private readonly ISubject<IList<VehicleType>> _vehiclesListSubject = new BehaviorSubject<IList<VehicleType>>(new List<VehicleType>());
		private MarketSettings _marketSettings = new MarketSettings();

		private readonly INetworkRoamingService _networkRoamingService;
		private readonly ICacheService _cacheService;

		private const string VehicleTypesDataCacheKey = "VehicleTypesData";

		public VehicleTypeService(INetworkRoamingService networkRoamingService, ICacheService cacheService)
		{
			_networkRoamingService = networkRoamingService;
			_cacheService = cacheService;

			Observe(networkRoamingService.GetAndObserveMarketSettings(), marketSettings => MarketChanged(marketSettings));
		}

		private async Task MarketChanged(MarketSettings marketSettings)
		{
			// If we changed market
			if (marketSettings.HashedMarket != _marketSettings.HashedMarket)
			{
				var vehicles = marketSettings.IsLocalMarket
					? await GetLocalMarketVehiclesList()
					: await GetExternalMarketVehicleTypes();

				_vehiclesListSubject.OnNext(vehicles);
			}

			_marketSettings = marketSettings;
		}

		public IObservable<IList<VehicleType>> GetAndObserveVehiclesList()
		{
			return _vehiclesListSubject;
		}

		public void ClearVehicleTypesCache()
		{
			_cacheService.Clear(VehicleTypesDataCacheKey);
		}

		private async Task<IList<VehicleType>> GetLocalMarketVehiclesList()
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

		private async Task<List<VehicleType>> GetExternalMarketVehicleTypes()
		{
			try
			{
				var position = _networkRoamingService.GetLastMarketChangedPositionTrigger();
				return await UseServiceClientAsync<NetworkRoamingServiceClient, List<VehicleType>>(service => service.GetExternalMarketVehicleTypes(position.Latitude, position.Longitude));
			}
			catch(Exception ex)
			{
				Logger.LogError(ex);
				return new List<VehicleType>();
			}
		}
	}
}

