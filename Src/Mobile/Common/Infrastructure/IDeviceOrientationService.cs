using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IDeviceOrientationService
	{
		bool IsAvailable();

		bool Start();

		bool Stop();

		/// <summary>
		/// Angles the changed event.
		/// </summary>
		/// <param name="angle">Angle is in degrees and assumed to be from 0 to 359 clockwise direction</param>
		void AngleChangedEvent(int angle);
	}
}