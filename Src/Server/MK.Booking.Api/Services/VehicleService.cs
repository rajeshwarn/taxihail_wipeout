#region

using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using AutoMapper;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class VehicleService : Service
    {
        private readonly IBookingWebServiceClient _bookingWebServiceClient;

        public VehicleService(IBookingWebServiceClient bookingWebServiceClient)
        {
            _bookingWebServiceClient = bookingWebServiceClient;
        }

        public AvailableVehiclesResponse Get(AvailableVehicles request)
        {
            var vehicles = _bookingWebServiceClient.GetAvailableVehicles(request.Latitude, request.Longitude);
            return new AvailableVehiclesResponse(vehicles.Select(Mapper.Map<AvailableVehicle>));
        }

        
    }
}