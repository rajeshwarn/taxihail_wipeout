namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IPackageInfo
    {
        string Platform { get; }
		string PlatformDetails { get; }
        string Version { get; }
        string UserAgent { get; }
    }
}
