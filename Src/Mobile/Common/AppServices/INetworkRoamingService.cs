
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface INetworkRoamingService
    {
        Task<string> GetLocalCompanyMarket(double latitude, double longitude);
    }
}