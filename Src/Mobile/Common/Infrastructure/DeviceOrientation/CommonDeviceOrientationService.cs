using System;
using apcurium.MK.Common;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Enumeration;
using apcurium.MK.Booking.Mobile.TaxihailEventArgs;
using Cirrious.MvvmCross.Platform;

namespace apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation
{
	public abstract class CommonDeviceOrientationService: IDeviceOrientationService
	{
        private readonly CoordinateSystemOrientation _coordinateSystemOrientation;
        private readonly IMvxLifetime _mvxLifetime;

        private const double RadiansToDegrees = 360 / (2 * Math.PI);
		private const double ThetaTrustedAngle = 40; // maximum angle in PI space between z axis of device and horizontal x-z plane when orientation events will be generated
		private const int TimeIntervalToGatherEventsSet = 250;

		private bool _isStarted;
		private readonly DeviceOrientationFilter _filter = new DeviceOrientationFilter();
		
        private DeviceOrientations _currentOrientation = DeviceOrientations.Up;

	    private readonly int[] _axes = { 45, 135, 225, 315 };
        private const int Deviation = 20;

        private readonly object _lock = new object();

	    public event EventHandler<DeviceOrientationChangedEventArgs> NotifyOrientationChanged;

        protected CommonDeviceOrientationService(CoordinateSystemOrientation coordinateSystemOrientation, IMvxLifetime mvxLifetime)
        {
            _coordinateSystemOrientation = coordinateSystemOrientation;
            _mvxLifetime = mvxLifetime;
        }

	    public bool Start()
		{
	        if (_isStarted || !IsAvailable())
	        {
	            return _isStarted;
	        }

	        _mvxLifetime.LifetimeChanged += OnApplicationLifetimeChanged;
	        _isStarted = StartService();

	        return _isStarted;
		}

		public bool Stop()
		{
			if (_isStarted && IsAvailable())
			{
				_isStarted = !StopService();
                _mvxLifetime.LifetimeChanged -= OnApplicationLifetimeChanged;
            }

			return !_isStarted;
		}

        private void OnApplicationLifetimeChanged(object sender, MvxLifetimeEventArgs args)
        {
            if (args.LifetimeEvent == MvxLifetimeEvent.ActivatedFromDisk || args.LifetimeEvent == MvxLifetimeEvent.ActivatedFromMemory)
            {
                _isStarted = StartService();
            }

            if (args.LifetimeEvent == MvxLifetimeEvent.Closing || args.LifetimeEvent == MvxLifetimeEvent.Deactivated)
            {
                _isStarted = !StopService();
            }
        }

        protected int GetZRotationAngle(Vector3 deviceOrientation)
		{
			var orientation = 1;

			if (_coordinateSystemOrientation == CoordinateSystemOrientation.LeftHanded)
			{
				orientation = -1;
			}

			var angle = 90 - (int)Math.Round(Math.Atan2(-deviceOrientation.y * orientation, deviceOrientation.x * orientation) * RadiansToDegrees);

			while (angle >= 360)
			{
				angle -= 360;
			}

			while (angle < 0)
			{
				angle += 360;
			}

			return angle;
		}

		/// <summary>
		/// When angle mesured in PI space between z axis and x-z plane is less then 40 degrees - returns true, otherwise false
		/// </summary>
		/// <param name="deviceOrientation">device spherical coordinates vector</param>
		/// <returns></returns>
		protected bool TrustZRotation(Vector3 deviceOrientation)
		{
			deviceOrientation.Normalize();
			var theta = Math.Asin(deviceOrientation.z) * RadiansToDegrees;

			return Math.Abs(theta) < ThetaTrustedAngle;
		}

		public abstract bool IsAvailable();

		protected abstract bool StartService();

		protected abstract bool StopService();

		/// <summary>
		/// timestamp of the event in milliseconds
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="timestamp"></param>
		public void OrientationChanged(double x, double y, double z, long timestamp)
		{
			var v = new Vector3(x, y, z);
			v.Normalize();

			var rotation = GetZRotationAngle(v);

			_filter.AddValue(rotation, DateTime.Now.Ticks / 10000);
			var filteredAngle = _filter.StatisticalFilter(TimeIntervalToGatherEventsSet);

		    AngleChanged(filteredAngle, filteredAngle != -1/* && trustZRotation*/);
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

        private void AngleChanged(int angle, bool trustData)
        {
            try
            {
                if (!trustData)
                {
                    return;
                }

                var deviceOrientation = GetOrientationByAngle(angle, _currentOrientation);

                if (_currentOrientation == deviceOrientation)
                {
                    return;
                }

                lock (_lock)
                {
                    _currentOrientation = deviceOrientation;
                }

                if (NotifyOrientationChanged != null)
                {
                    NotifyOrientationChanged(this, new DeviceOrientationChangedEventArgs() { DeviceOrientation = _currentOrientation });
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
            
        }
    }
}