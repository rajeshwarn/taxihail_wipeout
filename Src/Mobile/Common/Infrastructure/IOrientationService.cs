using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public enum DeviceOrientation
	{
		Up,
		Down,
		Left,
		Right
	}

	public interface IOrientationService
	{
		void Initialize(DeviceOrientation[] deviceOrientationNotifications);

		bool IsAvailable();

		bool Start();

		bool Stop();
	}
}