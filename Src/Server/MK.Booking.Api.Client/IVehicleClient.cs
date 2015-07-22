#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using System;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IVehicleClient
    {
        Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude, int? vehicleTypeId);

		Task<VehicleType[]> GetVehicleTypes();

        Task<EtaForPickupResponse> GetEtaFromGeo(double latitude, double longitude, string vehicleNumber, Guid orderId);

        Task SendMessageToDriver(string message, string vehicleNumber);
    }
}