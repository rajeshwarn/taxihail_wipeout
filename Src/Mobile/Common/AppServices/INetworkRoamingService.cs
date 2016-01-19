using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface INetworkRoamingService
    {
        Task<MarketSettings> GetHashedCompanyMarket(double latitude, double longitude);

        Task<List<NetworkFleet>> GetNetworkFleets();

        Task<List<VehicleType>> GetExternalMarketVehicleTypes(double latitude, double longitude);
    }
}