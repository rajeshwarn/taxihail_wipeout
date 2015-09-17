using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation
{
	public class OrientationService : IOrientationService
	{
		private AppServices.DeviceOrientation[] _deviceOrientationNotifications;
		private readonly IDeviceOrientationService _deviceOrientationService;
		private AppServices.DeviceOrientation _currentOrientation = AppServices.DeviceOrientation.Up;

	    private readonly int[] _axes = { 45, 135, 225, 315 };
		private const int Deviation = 20;

        private int _exclusiveAccess;
        private bool _initialized;
        private bool _started;

        public event Action<AppServices.DeviceOrientation> NotifyOrientationChanged;
        public event Action<int> NotifyAngleChanged;

		public OrientationService(IDeviceOrientationService deviceOrientationService)
		{
			this._deviceOrientationService = deviceOrientationService;
		}

		public void Initialize(AppServices.DeviceOrientation[] deviceOrientationNotifications)
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

		AppServices.DeviceOrientation GetOrientationByAngle(int angle, AppServices.DeviceOrientation currentDeviceOrientation)
		{
			int axe1, axe2, axe3, axe4;

			axe1 = _axes[0];
			axe2 = _axes[1];
			axe3 = _axes[2];
			axe4 = _axes[3];

			switch (currentDeviceOrientation)
			{
				case AppServices.DeviceOrientation.Up:
					axe1 += Deviation;
					axe4 -= Deviation;
					break;

				case AppServices.DeviceOrientation.Down:
					axe2 -= Deviation;
					axe3 += Deviation;
					break;

                case AppServices.DeviceOrientation.Right:
					axe1 -= Deviation;
					axe2 += Deviation;
					break;

                case AppServices.DeviceOrientation.Left:
					axe3 -= Deviation;
					axe4 += Deviation;
					break;
			}

			if (angle >= axe4 && angle <= axe1)
			{
				return AppServices.DeviceOrientation.Up;
			}

			if (angle > axe1 && angle <= axe2)
			{
				return AppServices.DeviceOrientation.Right;
			}

			if (angle > axe2 && angle < axe3)
			{
				return AppServices.DeviceOrientation.Down;
			}

			if (angle >= axe3 && angle < axe4)
			{
				return AppServices.DeviceOrientation.Left;
			}

			return AppServices.DeviceOrientation.Up;
		}

		public void AngleChanged(int angle)
		{
			if (NotifyAngleChanged != null)
			{
				NotifyAngleChanged(angle);
			}

			AppServices.DeviceOrientation deviceOrientation = GetOrientationByAngle(angle, _currentOrientation);

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