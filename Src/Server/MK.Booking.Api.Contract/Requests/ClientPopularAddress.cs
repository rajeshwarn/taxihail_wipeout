using System.Collections.Generic;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/popularaddresses", "GET")]
    [Route("/admin/popularaddresses", "GET")]
    public class ClientPopularAddress : BaseDTO
    {
        
    }

    [NoCache]
    public class ClientPopularAddressResponse : List<PopularAddressDetails>
    {
        public ClientPopularAddressResponse()
        {
            
        }

        public ClientPopularAddressResponse(IEnumerable<PopularAddressDetails> collection)
            :base(collection)
        {
            
        }
    }
}
