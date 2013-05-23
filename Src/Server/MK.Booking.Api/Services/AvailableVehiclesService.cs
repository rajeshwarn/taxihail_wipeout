using System.Linq;
using AutoMapper;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;

namespace apcurium.MK.Booking.Api.Services
{
    public class AvailableVehiclesService : Service
    {
        readonly IBookingWebServiceClient _bookingWebServiceClient;

        public AvailableVehiclesService(IBookingWebServiceClient bookingWebServiceClient)
        {
            _bookingWebServiceClient = bookingWebServiceClient;
        }

        public AvailableVehiclesResponse Get(AvailableVehicles request)
        {
            var vehicles = _bookingWebServiceClient.GetAvailableVehicles(request.Latitude, request.Longitude, 500, 10);
            
            return new AvailableVehiclesResponse(vehicles.Select(Mapper.Map<AvailableVehicle>));
        }
    }
}
