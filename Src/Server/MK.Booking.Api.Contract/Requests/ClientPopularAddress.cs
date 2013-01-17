using System.Collections.Generic;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Http;
#if !CLIENT
using apcurium.MK.Booking.ReadModel;
#endif

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/popularaddresses", "GET")]
    [Route("/admin/popularaddresses", "GET")]
    public class ClientPopularAddress : BaseDTO
    {
        
    }

#if !CLIENT
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
#endif
}
