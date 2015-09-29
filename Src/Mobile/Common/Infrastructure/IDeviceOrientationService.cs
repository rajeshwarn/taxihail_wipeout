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
		/// timestamp of the event in milliseconds
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="timestamp"></param>
		void OrientationChanged(double x, double y, double z, long timestamp);
	}
}