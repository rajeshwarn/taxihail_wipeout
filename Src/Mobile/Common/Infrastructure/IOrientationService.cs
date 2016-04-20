using System;
using apcurium.MK.Booking.Mobile.Enumeration;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IOrientationService
	{
		void Initialize(DeviceOrientations[] deviceOrientationsNotifications);

		bool IsAvailable();

		bool Start();

		bool Stop();

		event Action<DeviceOrientations> NotifyOrientationChanged;
		event Action<int, bool> NotifyAngleChanged;
	}
}