using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class CommonDeviceOrientationService
	{
		const double RadiansToDegrees = 360 / (2 * Math.PI);
		const double thetaTrustedAngle = 40;

		public event Action<int> NotifyAngleChanged;
		bool _isStarted = false;

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

		protected int GetZRotationAngle(double x, double y, double z)
		{
			int angle = 90 - (int)Math.Round(Math.Atan2(-y, x) * RadiansToDegrees);

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

		protected bool TrustZRotation(double x, double y, double z)
		{
			double theta = Math.Asin(ClampToOne(z)) * RadiansToDegrees;

			if (Math.Abs(theta) < thetaTrustedAngle)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		double ClampToOne(double value)
		{
			if (value > 1)
			{
				value = 1;
			}

			if (value < -1)
			{
				value = -1;
			}

			return value;
		}

		public abstract bool IsAvailable();

		protected abstract bool StartService();

		protected abstract bool StopService();

		/// <summary>
		/// Angles the changed event.
		/// </summary>
		/// <param name="angle">Angle is in degrees and assumed to be from 0 to 359 clockwise direction</param>
		public void AngleChangedEvent(int angle)
		{
			if (NotifyAngleChanged != null)
			{
				NotifyAngleChanged(angle);
			}
		}
	}
}