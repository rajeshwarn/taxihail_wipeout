using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
	[RouteDescription("/accounts/getconfirmationcode/{Email}/{CountryCode}/{PhoneNumber}", "GET")]
    public class ConfirmationCodeRequest
    {
        public string Email { get; set; }

		public string CountryCode { get; set; }

		public string PhoneNumber { get; set; }
    }
}