#region



#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/confirm/{EmailAddress}/{ConfirmationToken}", "GET")]
    public class ConfirmAccountRequest
    {
        public string EmailAddress { get; set; }
        public string ConfirmationToken { get; set; }
    }
}