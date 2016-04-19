using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/confirm/{EmailAddress}/{ConfirmationToken}/{IsSMSConfirmation*}", "GET")]
    public class ConfirmAccountRequest
    {
        public string EmailAddress { get; set; }
        public string ConfirmationToken { get; set; }
        public bool? IsSMSConfirmation { get; set; }
    }
}