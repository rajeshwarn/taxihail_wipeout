#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IVehicleClient
    {
        Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude);
    }
}