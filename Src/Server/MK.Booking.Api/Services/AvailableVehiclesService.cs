using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Services
{
    public class AvailableVehiclesService : Service
    {
        public AvailableVehiclesResponse Get(AvailableVehicles request)
        {

            return new AvailableVehiclesResponse();
        }
    }
}
