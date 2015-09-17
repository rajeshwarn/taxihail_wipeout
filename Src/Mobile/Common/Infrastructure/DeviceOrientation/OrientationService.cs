using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation
{
	public class OrientationService : IOrientationService
	{
		private DeviceOrientations[] _deviceOrientationsNotifications;
		private readonly IDeviceOrientationService _deviceOrientationService;
		private DeviceOrientations _currentOrientations = DeviceOrientations.Up;

	    private readonly int[] _axes = { 45, 135, 225, 315 };
		private const int Deviation = 20;

        private int _exclusiveAccess;
        private bool _initialized;
        private bool _started;

        public event Action<DeviceOrientations> NotifyOrientationChanged;
        public event Action<int> NotifyAngleChanged;

		public OrientationService(IDeviceOrientationService deviceOrientationService)
		{
			_deviceOrientationService = deviceOrientationService;
		}

		public void Initialize(DeviceOrientations[] deviceOrientationsNotifications)
		{
			if (!_initialized)
			{
				_deviceOrientationsNotifications = deviceOrientationsNotifications;
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

		DeviceOrientations GetOrientationByAngle(int angle, DeviceOrientations currentDeviceOrientations)
		{
		    var axe1 = _axes[0];
			var axe2 = _axes[1];
			var axe3 = _axes[2];
			var axe4 = _axes[3];

			switch (currentDeviceOrientations)
			{
				case DeviceOrientations.Up:
					axe1 += Deviation;
					axe4 -= Deviation;
					break;

				case DeviceOrientations.Down:
					axe2 -= Deviation;
					axe3 += Deviation;
					break;

                case DeviceOrientations.Right:
					axe1 -= Deviation;
					axe2 += Deviation;
					break;

                case DeviceOrientations.Left:
					axe3 -= Deviation;
					axe4 += Deviation;
					break;
			}

			if (angle >= axe4 && angle <= axe1)
			{
				return DeviceOrientations.Up;
			}

			if (angle > axe1 && angle <= axe2)
			{
				return DeviceOrientations.Right;
			}

			if (angle > axe2 && angle < axe3)
			{
				return DeviceOrientations.Down;
			}

			if (angle >= axe3 && angle < axe4)
			{
				return DeviceOrientations.Left;
			}

			return DeviceOrientations.Up;
		}

		public void AngleChanged(int angle)
		{
			if (NotifyAngleChanged != null)
			{
				NotifyAngleChanged(angle);
			}

			DeviceOrientations deviceOrientations = GetOrientationByAngle(angle, _currentOrientations);

			if (_currentOrientations != deviceOrientations)
			{
				if (System.Threading.Interlocked.CompareExchange(ref _exclusiveAccess, 1, 0) == 0)
				{
					if (_currentOrientations != deviceOrientations)
					{
						_currentOrientations = deviceOrientations;

						if (NotifyOrientationChanged != null && _deviceOrientationsNotifications.Contains(deviceOrientations))
						{
							NotifyOrientationChanged(_currentOrientations);
						}
					}

					_exclusiveAccess = 0;
				}
			}
		}
	}
}