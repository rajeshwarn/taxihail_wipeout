using apcurium.MK.Common.Enumeration;
using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IOrientationService
	{
		void Initialize(DeviceOrientations[] deviceOrientationsNotifications);

		bool IsAvailable();

		bool Start();

		bool Stop();

	    event EventHandler<DeviceOrientations> NotifyOrientationChanged;

		event Action<int, bool> NotifyAngleChanged;
	}
}