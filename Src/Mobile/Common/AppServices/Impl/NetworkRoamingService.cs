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
		private readonly ISubject<MarketSettings> _marketSettingsSubject = new BehaviorSubject<MarketSettings>(new MarketSettings { HashedMarket = null });

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

				var marketSettings = await GetMarketSettings(currentPosition.Latitude, currentPosition.Longitude);
				if (marketSettings == null)
				{
					// in case of no network we get null, init object with a non-null default value
					marketSettings = new MarketSettings();
				}

				_marketSettingsSubject.OnNext(marketSettings);
			}
		}

        public async Task<List<NetworkFleet>> GetNetworkFleets()
        {
			try
			{
				return await UseServiceClientAsync<NetworkRoamingServiceClient, List<NetworkFleet>>(service => service.GetNetworkFleets());
			}
			catch(Exception ex)
			{
				Logger.LogError(ex);
				return new List<NetworkFleet>();
			}
        }

		private async Task<MarketSettings> GetMarketSettings(double latitude, double longitude)
		{
			try
			{
				return await UseServiceClientAsync<NetworkRoamingServiceClient, MarketSettings>(service => service.GetCompanyMarketSettings(latitude, longitude));
			}
			catch(Exception ex)
			{
				Logger.LogError(ex);
				return new MarketSettings();
			}
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