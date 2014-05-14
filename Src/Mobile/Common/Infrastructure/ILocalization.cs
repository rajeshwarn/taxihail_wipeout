namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface ILocalization
	{
        string this[string key] { get; }
		bool Exists(string key);
		string CurrentLanguage { get; }
	}
}

