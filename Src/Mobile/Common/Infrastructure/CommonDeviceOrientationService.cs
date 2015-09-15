using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public enum CoordinateSystemOrientation
	{
		LeftHanded,
		RightHanded
	}

	public abstract class CommonDeviceOrientationService
	{
		public struct Vector3
		{
			public double x, y, z;

			public Vector3(double x, double y, double z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}

			public void Normalize()
			{
				double vectorLength = Math.Pow(x * x + y * y + z * z, 0.5);

				x /= vectorLength;
				y /= vectorLength;
				z /= vectorLength;
			}
		}

		class Filter
		{
			struct FilterValue
			{
				public int value;
				public long timestamp;
			}

			FilterValue[] _buffer = new FilterValue[1000];
			int _pointer = -1;
			int _bufferLength = 0;
			const int MaximumRandomDeviation = 10;

			public void AddValue(int value, long time)
			{
				_pointer++;
				if (_pointer == 1000)
					_pointer = 0;

				_buffer[_pointer].value = value;
				_buffer[_pointer].timestamp = time;

				if (_bufferLength < 1000)
					_bufferLength++;
			}

			FilterValue ReadValueFromEnd(int position)
			{
				while (position >= _bufferLength)
				{
					position -= _bufferLength;
				}

				while (position < 0)
				{
					position += _bufferLength;
				}

				int positionFromBufferStartPointer = _pointer - position;

				if (positionFromBufferStartPointer < 0)
					positionFromBufferStartPointer += _bufferLength;

				return _buffer[positionFromBufferStartPointer];
			}

			public int StatisticalFilter()
			{
				if (_bufferLength == 0)
					return -1;

				bool timeAchieved = false;
				long time = 0;

				FilterValue fv1 = ReadValueFromEnd(0);
				FilterValue fv2 = ReadValueFromEnd(0);

				int maximumValueDifference = 0;

				for (int i = 0; i < _bufferLength; i++)
				{
					fv2 = ReadValueFromEnd(i);

					time += fv1.timestamp - fv2.timestamp;

					int valueDifference = Math.Abs(fv2.value - fv1.value);

					if (valueDifference > 180)
						valueDifference = 360 - valueDifference;

					maximumValueDifference = Math.Max(maximumValueDifference, valueDifference);

					fv1 = fv2;

					if (time > 250)
					{
						timeAchieved = true;
						break;
					}
				}

				if (!timeAchieved)
				{
					fv1 = ReadValueFromEnd(0);
					fv2 = ReadValueFromEnd(0);

					for (int i = 0; i < Math.Min(_bufferLength, 5); i++)
					{
						fv2 = ReadValueFromEnd(i);

						int valDiff2 = Math.Abs(fv2.value - fv1.value);

						if (valDiff2 > 180)
							valDiff2 = 360 - valDiff2;

						maximumValueDifference = Math.Max(maximumValueDifference, valDiff2);
					}
				}

				if (maximumValueDifference <= MaximumRandomDeviation)
				{
					return ReadValueFromEnd(0).value;
				}
				else
				{
					return -1;
				}
			}
		}

		const double RadiansToDegrees = 360 / (2 * Math.PI);
		const double thetaTrustedAngle = 40;

		public event Action<int> NotifyAngleChanged;
		bool _isStarted = false;
		Filter filter = new Filter();
		CoordinateSystemOrientation _coordinateSystemOrientation;

		public CommonDeviceOrientationService(CoordinateSystemOrientation coordinateSystemOrientation)
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

		protected int GetZRotationAngle(Vector3 vector)
		{
			int orientation = 1;

			if (_coordinateSystemOrientation == CoordinateSystemOrientation.LeftHanded)
				orientation = -1;

			int angle = 90 - (int)Math.Round(Math.Atan2(-vector.y * orientation, vector.x * orientation) * RadiansToDegrees);

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

		protected bool TrustZRotation(Vector3 vector)
		{
			vector.Normalize();
			double theta = Math.Asin(vector.z) * RadiansToDegrees;

			if (Math.Abs(theta) < thetaTrustedAngle)
			{
				return true;
			}
			else
			{
				return false;
			}
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
			Vector3 v = new Vector3(x, y, z);
			v.Normalize();

			if (TrustZRotation(v))
			{
				int rotation = GetZRotationAngle(v);

				filter.AddValue(rotation, timestamp);

				int filteredAngle = filter.StatisticalFilter();

				if (NotifyAngleChanged != null && filteredAngle != -1)
				{
					NotifyAngleChanged(filteredAngle);
				}
			}
		}
	}
}