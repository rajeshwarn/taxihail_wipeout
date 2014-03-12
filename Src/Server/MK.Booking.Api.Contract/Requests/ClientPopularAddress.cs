using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Http;

#region

#if !CLIENT
using apcurium.MK.Booking.ReadModel;
#endif

#endregion

using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/popularaddresses", "GET")]
    [Route("/admin/popularaddresses", "GET")]
    public class ClientPopularAddress : BaseDto
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
            : base(collection)
        {
        }
    }
#endif
}