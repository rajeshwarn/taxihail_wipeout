using apcurium.MK.Common;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[Route("/account/getconfirmationcode/{Email}/{CountryCode}/{PhoneNumber}", "GET")]
    public class ConfirmationCodeRequest
    {
        public string Email { get; set; }

		public string CountryCode { get; set; }

		public string PhoneNumber { get; set; }
    }
}