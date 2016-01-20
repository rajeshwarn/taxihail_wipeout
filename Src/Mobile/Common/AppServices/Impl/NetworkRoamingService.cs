using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Reactive.Subjects;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class NetworkRoamingService : BaseService, INetworkRoamingService
    {
		private readonly ISubject<MarketSettings> _marketSettingsSubject = new BehaviorSubject<MarketSettings>(new MarketSettings());

		private Position _lastMarketPosition = new Position();

		private const int LastMarketDistanceThresholdInMeters = 1000;

		public IObservable<MarketSettings> GetAndObserveMarketSettings()
		{
			return _marketSettingsSubject;
		}

		public Position GetLastMarketChangedPositionTrigger()
		{
			return _lastMarketPosition;
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

		private bool ShouldUpdateMarket(Position currentPosition)
		{
			var distanceFromLastMarketRequest = Maps.Geo.Position.CalculateDistance(
				currentPosition.Latitude, currentPosition.Longitude,
				_lastMarketPosition.Latitude, _lastMarketPosition.Longitude);

			return distanceFromLastMarketRequest > LastMarketDistanceThresholdInMeters;
		}
    }
}