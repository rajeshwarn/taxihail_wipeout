using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/account/phone/{Email}", "GET")]
	public class CurrentAccountPhoneRequest : BaseDto
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