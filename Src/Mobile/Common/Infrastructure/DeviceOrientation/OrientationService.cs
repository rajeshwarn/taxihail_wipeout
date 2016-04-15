using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.TaxihailEventArgs;
using apcurium.MK.Common.Enumeration;
using Cirrious.MvvmCross.Platform;

namespace apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation
{
	public class OrientationService : IOrientationService
	{
		private readonly IDeviceOrientationService _deviceOrientationService;
		private readonly IMvxLifetime _mvxLifetime;

		private DeviceOrientations[] _deviceOrientationsNotifications;
		private DeviceOrientations _currentOrientation = DeviceOrientations.Up;

		bool _previousTrustZRotation = true;
		bool _currentTrustZRotation = false;

		private readonly int[] _axes = { 45, 135, 225, 315 };
		private const int Deviation = 20;

		private int _exclusiveAccess;
		private bool _initialized;
		private bool _started;

		public event EventHandler<DeviceOrientationChangedEventArgs> NotifyOrientationChanged;
		public event Action<int, bool> NotifyAngleChanged;

		public OrientationService(IDeviceOrientationService deviceOrientationService, IMvxLifetime mvxLifetime)
		{
			_deviceOrientationService = deviceOrientationService;
			_mvxLifetime = mvxLifetime;
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
				_mvxLifetime.LifetimeChanged += OnApplicationLifetimeChanged;
				_previousTrustZRotation = true;
				_started = true;
				_deviceOrientationService.NotifyAngleChanged += AngleChanged;
				_started = _deviceOrientationService.Start();

				return _started;
			}

			return false;
		}

		public bool Stop()
		{
			if (_initialized && _started)
			{
				_mvxLifetime.LifetimeChanged -= OnApplicationLifetimeChanged;
				_started = false;
				_deviceOrientationService.Stop();
				((CommonDeviceOrientationService)_deviceOrientationService).NotifyAngleChanged -= AngleChanged;

				return true;
			}

			return false;
		}

		private void OnApplicationLifetimeChanged(object sender, MvxLifetimeEventArgs args)
		{
			if (args.LifetimeEvent == MvxLifetimeEvent.ActivatedFromDisk || args.LifetimeEvent == MvxLifetimeEvent.ActivatedFromMemory)
			{
				_deviceOrientationService.Start();
			}

			if (args.LifetimeEvent == MvxLifetimeEvent.Closing || args.LifetimeEvent == MvxLifetimeEvent.Deactivated)
			{
				_deviceOrientationService.Stop();
			}
		}

		private DeviceOrientations GetOrientationByAngle(int angle, DeviceOrientations currentDeviceOrientations)
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

		public void AngleChanged(int angle, bool trustData, bool trustZRotation)
		{
			if (trustData)
			{
				_previousTrustZRotation = _currentTrustZRotation;
				_currentTrustZRotation = trustZRotation;
			}
			else
			{
				_currentTrustZRotation = false;
				return;
			}

			if (NotifyAngleChanged != null)
			{
				NotifyAngleChanged(angle, trustZRotation);
			}

			var deviceOrientation = GetOrientationByAngle(angle, _currentOrientation);

			if (_currentOrientation != deviceOrientation || (_currentTrustZRotation && !_previousTrustZRotation))
			{
				if (System.Threading.Interlocked.CompareExchange(ref _exclusiveAccess, 1, 0) == 0)
				{
					if (_currentOrientation != deviceOrientation || (_currentTrustZRotation && !_previousTrustZRotation))
					{
						_currentOrientation = deviceOrientation;

						if (NotifyOrientationChanged != null && _deviceOrientationsNotifications.Contains(deviceOrientation))
						{
							NotifyOrientationChanged(this, new DeviceOrientationChangedEventArgs() {DeviceOrientation = _currentOrientation });
						}
					}

					_exclusiveAccess = 0;
				}
			}
		}
	}
}