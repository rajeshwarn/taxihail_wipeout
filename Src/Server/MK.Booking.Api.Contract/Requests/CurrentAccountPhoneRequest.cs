using apcurium.MK.Common;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/accounts/phone/{Email}", "GET")]
	public class CurrentAccountPhoneRequest:IReturn<CurrentAccountPhoneResponse>
	{
		public string Email { get; set; }
	}
    
	public class CurrentAccountPhoneResponse
	{
		public CountryISOCode CountryCode { get; set; }

		public string PhoneNumber { get; set; }
	}
}