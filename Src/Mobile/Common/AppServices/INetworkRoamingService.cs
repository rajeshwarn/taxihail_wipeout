using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface INetworkRoamingService
    {
		IObservable<MarketSettings> GetAndObserveMarketSettings();

		Task UpdateMarketSettingsIfNecessary(Position currentPosition);

        Task<List<NetworkFleet>> GetNetworkFleets();

		Position GetLastMarketChangedPositionTrigger();
    }
}