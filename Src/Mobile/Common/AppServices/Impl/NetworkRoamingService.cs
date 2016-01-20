using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Reactive.Subjects;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class NetworkRoamingService : BaseService, INetworkRoamingService
    {
		private readonly ISubject<MarketSettings> _marketSettingsSubject = new BehaviorSubject<MarketSettings>(new MarketSettings());

		private List<VehicleType> _marketVehicleTypes = new List<VehicleType>();
		private Position _lastMarketPosition = new Position();

		private const int LastMarketDistanceThresholdInMeters = 1000;

		public IObservable<MarketSettings> GetAndObserveMarketSettings()
		{
			return _marketSettingsSubject;
		}

		public async Task UpdateMarketSettingsIfNecessary(Position currentPosition)
		{
			if (ShouldUpdateMarket(currentPosition))
			{
				_lastMarketPosition = currentPosition;

				var marketSettings = await GetHashedCompanyMarket(currentPosition.Latitude, currentPosition.Longitude);
				if (marketSettings == null)
				{
					// in case of no network we get null, init object with a non-null default value
					marketSettings = new MarketSettings();
				}

				var previousMarketSettings = await _marketSettingsSubject.Take(1).ToTask();
				if (previousMarketSettings.HashedMarket != marketSettings.HashedMarket)
				{
					if (marketSettings.IsLocalMarket)
					{
						_marketVehicleTypes = new List<VehicleType>();
					}
					else
					{
						// If market has changed, we need to get the external market vehicle types.
						// Do this before changing the market settings subject so observing classes can call 
						// GetExternalMarketVehicleTypes() immediately when notified of market settings change
						_marketVehicleTypes = await GetExternalMarketVehicleTypes(currentPosition.Latitude, currentPosition.Longitude);
					}
				}

				_marketSettingsSubject.OnNext(marketSettings);
			}
		}

        public Task<List<NetworkFleet>> GetNetworkFleets()
        {
			var tcs = new TaskCompletionSource<List<NetworkFleet>>();

			try
			{
				var result =
					UseServiceClientAsync<NetworkRoamingServiceClient, List<NetworkFleet>>(
						service => service.GetNetworkFleets()).Result;
				tcs.TrySetResult(result);
			}
			catch
			{
				tcs.TrySetResult(new List<NetworkFleet>());
			}

			return tcs.Task;
        }

        public List<VehicleType> GetExternalMarketVehicleTypes()
        {
			return _marketVehicleTypes;
        }

		private Task<MarketSettings> GetHashedCompanyMarket(double latitude, double longitude)
		{
			var tcs = new TaskCompletionSource<MarketSettings>();

			try
			{
				var result = UseServiceClientAsync<NetworkRoamingServiceClient, MarketSettings>(service => service.GetCompanyMarketSettings(latitude, longitude)).Result;
				tcs.TrySetResult(result);
			}
			catch
			{
				tcs.TrySetResult(new MarketSettings());
			}

			return tcs.Task;
		}

		private Task<List<VehicleType>> GetExternalMarketVehicleTypes(double latitude, double longitude)
		{
			var tcs = new TaskCompletionSource<List<VehicleType>>();

			try
			{
				var result =
					UseServiceClientAsync<NetworkRoamingServiceClient, List<VehicleType>>(
						service => service.GetExternalMarketVehicleTypes(latitude, longitude)).Result;
				tcs.TrySetResult(result);
			}
			catch
			{
				tcs.TrySetResult(new List<VehicleType>());
			}

			return tcs.Task;
		}

		private bool ShouldUpdateMarket(Position currentPosition)
		{
			var distanceFromLastMarketRequest = Maps.Geo.Position.CalculateDistance(
				currentPosition.Latitude, currentPosition.Longitude,
				_lastMarketPosition.Latitude, _lastMarketPosition.Longitude);

			return distanceFromLastMarketRequest > LastMarketDistanceThresholdInMeters;
		}
    }
}