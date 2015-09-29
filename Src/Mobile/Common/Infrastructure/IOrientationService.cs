using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IOrientationService
	{
		void Initialize(DeviceOrientations[] deviceOrientationsNotifications);

		bool IsAvailable();

		bool Start();

		bool Stop();
	}
}