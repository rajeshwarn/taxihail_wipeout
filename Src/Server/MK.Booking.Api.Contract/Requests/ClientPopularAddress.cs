﻿#region


#if !CLIENT
using apcurium.MK.Booking.ReadModel;

#endif

#endregion

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