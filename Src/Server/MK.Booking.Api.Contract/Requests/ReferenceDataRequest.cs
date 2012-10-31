using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/referencedata", "GET")]
    public class ReferenceDataRequest : BaseDTO
    {
        public bool WithoutFiltering { get; set; }
    }
}