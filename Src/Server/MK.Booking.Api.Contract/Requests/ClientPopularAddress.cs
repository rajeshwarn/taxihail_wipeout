using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Entity;

#region

#if !CLIENT
using apcurium.MK.Booking.ReadModel;
#endif

#endregion

using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/popularaddresses", "GET")]
    public class ClientPopularAddress : BaseDto
    {
    }


#if !CLIENT


	[Route("/admin/popularaddresses", "GET")]
	public class AdminPopularAddress : BaseDto
	{
	}


	[NoCache]
	public class AdminPopularAddressResponse : List<PopularAddressDetails>
	{
		public AdminPopularAddressResponse()
		{
		}

		public AdminPopularAddressResponse(IEnumerable<PopularAddressDetails> collection)
			: base(collection)
		{
		}
	}

    [NoCache]
	public class ClientPopularAddressResponse : List<Address>
    {
        public ClientPopularAddressResponse()
        {
        }

		public ClientPopularAddressResponse(IEnumerable<Address> collection)
            : base(collection)
        {
        }
    }
#endif
}