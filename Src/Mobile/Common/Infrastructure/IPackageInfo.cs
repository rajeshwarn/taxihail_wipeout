namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IPackageInfo
    {
        string Platform
        {
            get;     
        }

        string Version { get; }
        string UserAgent { get; }
    }
}
