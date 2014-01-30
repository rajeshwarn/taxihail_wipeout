using apcurium.MK.Booking.Mobile.Settings;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAppSettings
	{
		TaxiHailSetting Data { get; }
		void Load();
	}
}

