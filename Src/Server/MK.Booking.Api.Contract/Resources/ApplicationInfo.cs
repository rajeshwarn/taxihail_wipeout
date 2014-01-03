namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class ApplicationInfo : BaseDto
    {
        public string Version { get; set; }
        public string SiteName { get; set; }
    }
}