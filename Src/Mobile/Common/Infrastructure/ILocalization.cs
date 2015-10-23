namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface ILocalization
	{
        string this[string key, string context = string.Empty] { get; }
		bool Exists(string key);
		string CurrentLanguage { get; }
		bool IsRightToLeft { get; }
	}
}

