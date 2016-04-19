using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;
#if !CLIENT
using apcurium.MK.Booking.ReadModel;
#endif


namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/popularaddresses", "GET")]
    public class ClientPopularAddress : BaseDto
    {
    }


#if !CLIENT

	[RouteDescription("/admin/popularaddresses", "GET")]
	public class AdminPopularAddress : BaseDto
	{
	}
	
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