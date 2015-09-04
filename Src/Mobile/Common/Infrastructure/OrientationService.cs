using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class OrientationService : IOrientationService
	{
		private DeviceOrientation[] _deviceOrientationNotifications;
		private IDeviceOrientationService _deviceOrientationService;
		private DeviceOrientation _currentOrientation = DeviceOrientation.Up;

		int[] _axes = { 45, 135, 225, 315 };
		const int Deviation = 20;

		public event Action<DeviceOrientation> NotifyOrientationChanged;
		public event Action<int> NotifyAngleChanged;

		int _exclusiveAccess = 0;
		bool _initialized = false;
		bool _started = false;

		public OrientationService(IDeviceOrientationService deviceOrientationService)
		{
			this._deviceOrientationService = deviceOrientationService;
		}

		public void Initialize(DeviceOrientation[] deviceOrientationNotifications)
		{
			if (!_initialized)
			{
				this._deviceOrientationNotifications = deviceOrientationNotifications;
				_initialized = true;
			}
		}

		public bool IsAvailable()
		{
			return _deviceOrientationService.IsAvailable();
		}

		public bool Start()
		{
			if (_initialized && !_started && IsAvailable())
			{
				_started = true;
				((CommonDeviceOrientationService)_deviceOrientationService).NotifyAngleChanged += AngleChanged;
				_started = _deviceOrientationService.Start();

				return _started;
			}

			return false;
		}

		public bool Stop()
		{
			if (_initialized && _started)
			{
				_started = false;
				_deviceOrientationService.Stop();
				((CommonDeviceOrientationService)_deviceOrientationService).NotifyAngleChanged -= AngleChanged;

				return true;
			}

			return false;
		}

		DeviceOrientation GetOrientationByAngle(int angle, DeviceOrientation currentDeviceOrientation)
		{
			var axes = new int[4];
			Array.Copy(_axes, axes, 4);

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
			{
				return DeviceOrientation.Up;
			}

			if (angle > axes[0] && angle <= axes[1])
			{
				return DeviceOrientation.Right;
			}

			if (angle > axes[1] && angle < axes[2])
			{
				return DeviceOrientation.Down;
			}

			if (angle >= axes[2] && angle < axes[3])
			{
				return DeviceOrientation.Left;
			}

			return DeviceOrientation.Up;
		}

		public void AngleChanged(int angle)
		{
			if (NotifyAngleChanged != null)
			{
				NotifyAngleChanged(angle);
			}

			DeviceOrientation deviceOrientation = GetOrientationByAngle(angle, _currentOrientation);

			if (_currentOrientation != deviceOrientation)
			{
				if (System.Threading.Interlocked.CompareExchange(ref _exclusiveAccess, 1, 0) == 0)
				{
					if (_currentOrientation != deviceOrientation)
					{
						_currentOrientation = deviceOrientation;

						if (NotifyOrientationChanged != null && _deviceOrientationNotifications.Contains(deviceOrientation))
						{
							NotifyOrientationChanged(_currentOrientation);
						}
					}

					_exclusiveAccess = 0;
				}
			}
		}
	}
}