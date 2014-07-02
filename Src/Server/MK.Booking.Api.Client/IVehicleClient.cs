#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IVehicleClient
    {
        Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude, int vehicleTypeId);
		Task<VehicleType[]> GetVehicleTypes();
    }
}