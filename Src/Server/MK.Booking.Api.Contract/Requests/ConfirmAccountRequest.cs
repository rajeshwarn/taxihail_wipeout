#region

using ServiceStack.ServiceHost;

#endregion



namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/confirm/{EmailAddress}/{ConfirmationToken}/{IsSMSConfirmation}", "GET")]
    public class ConfirmAccountRequest
    {
        public string EmailAddress { get; set; }
        public string ConfirmationToken { get; set; }
        public bool IsSMSConfirmation { get; set; }
    }
}