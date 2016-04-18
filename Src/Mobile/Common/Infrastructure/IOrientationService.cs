using apcurium.MK.Common.Enumeration;
using System;
using apcurium.MK.Booking.Mobile.TaxihailEventArgs;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IOrientationService
	{
		void Initialize(DeviceOrientations[] deviceOrientationsNotifications);

		bool IsAvailable();

		bool Start();

		bool Stop();

	    event EventHandler<DeviceOrientationChangedEventArgs> NotifyOrientationChanged;
	}
}