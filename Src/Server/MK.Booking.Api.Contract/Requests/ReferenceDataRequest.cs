using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/referencedata", "GET")]    
    public class ReferenceDataRequest
    {
         
    }
}