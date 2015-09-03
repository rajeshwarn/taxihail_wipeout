using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class OrientationService : IOrientationService
	{
		DeviceOrientation[] deviceOrientationNotifications;
		IDeviceOrientationService deviceOrientationService;
		DeviceOrientation currentOrientation = DeviceOrientation.Up;

		int[] Axes = { 45, 135, 225, 315 };
		int Deviation = 20;

		public event Action<DeviceOrientation> NotifyOrientationChanged;
		public event Action<int> NotifyAngleChanged;

		int exclusiveAccess = 0;
		bool initialized = false, started = false;

		public OrientationService(IDeviceOrientationService deviceOrientationService)
		{
			this.deviceOrientationService = deviceOrientationService;
		}

		public void Initialize(DeviceOrientation[] deviceOrientationNotifications)
		{
			if (!initialized)
			{
				this.deviceOrientationNotifications = deviceOrientationNotifications;
				initialized = true;
			}
		}

		public bool IsAvailable()
		{
			return deviceOrientationService.IsAvailable();
		}

		public bool Start()
		{
			if (initialized && !started && IsAvailable())
			{
				started = true;
				((CommonDeviceOrientationService)deviceOrientationService).NotifyAngleChanged += AngleChanged;
				started = deviceOrientationService.Start();

				return started;
			}

			return false;
		}

		public bool Stop()
		{
			if (initialized && started)
			{
				started = false;
				deviceOrientationService.Stop();
				((CommonDeviceOrientationService)deviceOrientationService).NotifyAngleChanged -= AngleChanged;

				return true;
			}

			return false;
		}

		DeviceOrientation GetOrientationByAngle(int angle, DeviceOrientation currentDeviceOrientation)
		{
			int[] axes = new int[4];
			Array.Copy(Axes, axes, 4);

			switch (currentDeviceOrientation)
			{
				case DeviceOrientation.Up:
					axes[0] += Deviation;
					axes[3] -= Deviation;
					break;

				case DeviceOrientation.Down:
					axes[1] -= Deviation;
					axes[2] += Deviation;
					break;

                case DeviceOrientation.Right:
					axes[0] -= Deviation;
					axes[1] += Deviation;
					break;

                case DeviceOrientation.Left:
					axes[2] -= Deviation;
					axes[3] += Deviation;
					break;
			}

			if (angle >= axes[3] && angle <= axes[0])
				return DeviceOrientation.Up;

			if (angle > axes[0] && angle <= axes[1])
                return DeviceOrientation.Right;

			if (angle > axes[1] && angle < axes[2])
				return DeviceOrientation.Down;

			if (angle >= axes[2] && angle < axes[3])
                return DeviceOrientation.Left;

			return DeviceOrientation.Up;
		}

		public void AngleChanged(int angle)
		{
			if (NotifyAngleChanged != null)
				NotifyAngleChanged(angle);

			DeviceOrientation deviceOrientation = GetOrientationByAngle(angle, currentOrientation);

			if (currentOrientation != deviceOrientation)
			{
				if (System.Threading.Interlocked.CompareExchange(ref exclusiveAccess, 1, 0) == 0)
				{
					if (currentOrientation != deviceOrientation)
					{
						currentOrientation = deviceOrientation;

						if (NotifyOrientationChanged != null && deviceOrientationNotifications.Contains(deviceOrientation))
							NotifyOrientationChanged(currentOrientation);
					}

					exclusiveAccess = 0;
				}
			}
		}
	}
}