using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class CommonDeviceOrientationService
	{
		public event Action<int> NotifyAngleChanged;
		bool started = false;

		public bool Start()
		{
			if (!started && IsAvailable())
				started = StartService();

			return started;
		}

		public bool Stop()
		{
			if (started && IsAvailable())
				started = !StopService();

			return !started;
		}

		public abstract bool IsAvailable();

		protected abstract bool StartService();

		protected abstract bool StopService();

		public void AngleChangedEvent(int angle)
		{
			if (NotifyAngleChanged != null)
				NotifyAngleChanged(angle);
		}
	}
}