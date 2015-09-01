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
	}
}