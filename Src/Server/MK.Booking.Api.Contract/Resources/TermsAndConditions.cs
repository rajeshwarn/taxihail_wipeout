namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class TermsAndConditions
    {
        public string Content { get; set; }

        //prop for client to detect if terms has been updated
        public bool Updated { get; set; }
    }
}