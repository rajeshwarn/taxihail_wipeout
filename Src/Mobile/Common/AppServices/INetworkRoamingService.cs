using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface INetworkRoamingService
    {
        Task<string> GetCompanyMarket(double latitude, double longitude);
    }
}