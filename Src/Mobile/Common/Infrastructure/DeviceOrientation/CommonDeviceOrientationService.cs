using System;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation
{
	public abstract class CommonDeviceOrientationService
	{
		private const double RadiansToDegrees = 360 / (2 * Math.PI);
		private const double ThetaTrustedAngle = 40; // maximum angle in PI space between z axis of device and horizontal x-z plane when orientation events will be generated
		private const int TimeIntervalToGatherEventsSet = 250;

		private bool _isStarted;
        private readonly DeviceOrientationFilter _filter = new DeviceOrientationFilter();
	    private readonly CoordinateSystemOrientation _coordinateSystemOrientation;

		public event Action<int> NotifyAngleChanged;

	    protected CommonDeviceOrientationService(CoordinateSystemOrientation coordinateSystemOrientation)
		{
			_coordinateSystemOrientation = coordinateSystemOrientation;
		}

		public bool Start()
		{
			if (!_isStarted && IsAvailable())
			{
				_isStarted = StartService();
			}

			return _isStarted;
		}

		public bool Stop()
		{
			if (_isStarted && IsAvailable())
			{
				_isStarted = !StopService();
			}

			return !_isStarted;
		}

		protected int GetZRotationAngle(Vector3 deviceOrientation)
		{
			int orientation = 1;

		    if (_coordinateSystemOrientation == CoordinateSystemOrientation.LeftHanded)
		    {
                orientation = -1;
		    }

			int angle = 90 - (int)Math.Round(Math.Atan2(-deviceOrientation.y * orientation, deviceOrientation.x * orientation) * RadiansToDegrees);

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
			double theta = Math.Asin(deviceOrientation.z) * RadiansToDegrees;

			if (Math.Abs(theta) < ThetaTrustedAngle)
			{
				return true;
			}
		    return false;
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

			if (TrustZRotation(v))
			{
				int rotation = GetZRotationAngle(v);

				_filter.AddValue(rotation, DateTime.Now.Ticks / 10000);

				int filteredAngle = _filter.StatisticalFilter(TimeIntervalToGatherEventsSet);

				if (NotifyAngleChanged != null && filteredAngle != -1)
				{
					NotifyAngleChanged(filteredAngle);
				}
			}
		}
	}
}