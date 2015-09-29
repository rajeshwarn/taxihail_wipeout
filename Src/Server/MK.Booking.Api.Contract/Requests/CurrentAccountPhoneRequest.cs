using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/account/phone/{Email}", "GET")]
	public class CurrentAccountPhoneRequest:IReturn<CurrentAccountPhoneResponse>
	{
		public string Email { get; set; }
	}

	[NoCache]
	public class CurrentAccountPhoneResponse
	{
		public CountryISOCode CountryCode { get; set; }

		public string PhoneNumber { get; set; }
	}
}