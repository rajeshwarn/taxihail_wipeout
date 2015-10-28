#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using System;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IVehicleClient
    {
        Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude, int? vehicleTypeId, ServiceType? serviceType);

		Task<VehicleType[]> GetVehicleTypes();
        
        Task<EtaForPickupResponse> GetEtaFromGeo(double latitude, double longitude, string vehicleRegistration, Guid orderId);

	    Task<AvailableVehicle> GetTaxiLocation(Guid orderId, string medallion);

		Task SendMessageToDriver(string message, string vehicleNumber, Guid orderId, ServiceType serviceType);
    }
}