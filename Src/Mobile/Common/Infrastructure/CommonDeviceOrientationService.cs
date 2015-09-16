using apcurium.MK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class CommonDeviceOrientationService
	{
		class Filter
		{
			struct FilterValue
			{
				public int value;
				public long timestamp;
			}

			private const int BufferCapacity = 10;
			private const int MaximumRandomDeviation = 10;

			private FilterValue[] _buffer = new FilterValue[BufferCapacity];
			private int _bufferTopPointer = -1;
			private int _bufferLength = 0;
			private int _exclusiveAccess = 0;

			public void AddValue(int value, long time)
			{
				if (System.Threading.Interlocked.CompareExchange(ref _exclusiveAccess, 1, 0) == 0)
				{
					_bufferTopPointer++;

					if (_bufferTopPointer == BufferCapacity)
					{
						_bufferTopPointer = 0;
					}

					_buffer[_bufferTopPointer].value = value;
					_buffer[_bufferTopPointer].timestamp = time;

					if (_bufferLength < BufferCapacity)
					{
						_bufferLength++;
					}

					_exclusiveAccess = 0;
				}
			}

			FilterValue ReadValueFromEnd(int position)
			{
				FilterValue result = new FilterValue();

				while (position >= _bufferLength)
				{
					position -= _bufferLength;
				}

				while (position < 0)
				{
					position += _bufferLength;
				}

				int positionFromBufferStartPointer = _bufferTopPointer - position;

				if (positionFromBufferStartPointer < 0)
				{
					positionFromBufferStartPointer += _bufferLength;
				}

				result = _buffer[positionFromBufferStartPointer];


				return result;
			}

			/// <summary>
			/// Takes the events of the last ~250 milliseconds or, if this time is not be able to gather, the last 5 events,
			/// if deviation between neighbour events in this set is less or equal MaximumRandomDeviation - returns value of the last event, otherwise [-1]
			/// </summary>
			/// <returns></returns>
			public int StatisticalFilter(int timeIntervalToGatherEventsSet)
			{
				int result = -1;

				if (System.Threading.Interlocked.CompareExchange(ref _exclusiveAccess, 1, 0) == 0)
				{
					if (_bufferLength > 0)
					{
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
							{
								valueDifference = 360 - valueDifference;
							}

							maximumValueDifference = Math.Max(maximumValueDifference, valueDifference);

							fv1 = fv2;

							if (time > timeIntervalToGatherEventsSet)
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

								int valueDifference = Math.Abs(fv2.value - fv1.value);

								if (valueDifference > 180)
								{
									valueDifference = 360 - valueDifference;
								}

								maximumValueDifference = Math.Max(maximumValueDifference, valueDifference);
							}
						}

						if (maximumValueDifference <= MaximumRandomDeviation)
						{
							result = ReadValueFromEnd(0).value;
						}
						else
						{
							result = -1;
						}
					}

					_exclusiveAccess = 0;
				}

				return result;
			}
		}

		private const double RadiansToDegrees = 360 / (2 * Math.PI);
		private const double ThetaTrustedAngle = 40; // maximum angle in PI space between z axis of device and horizontal x-z plane when orientation events will be generated
		private const int TimeIntervalToGatherEventsSet = 250;

		private bool _isStarted = false;
		private Filter _filter = new Filter();
		CoordinateSystemOrientation _coordinateSystemOrientation;

		public event Action<int> NotifyAngleChanged;

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

		protected int GetZRotationAngle(Vector3 deviceOrientation)
		{
			int orientation = 1;

			if (_coordinateSystemOrientation == CoordinateSystemOrientation.LeftHanded)
				orientation = -1;

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

				_filter.AddValue(rotation, (long)(DateTime.Now.Ticks / 10000));

				int filteredAngle = _filter.StatisticalFilter(TimeIntervalToGatherEventsSet);

				if (NotifyAngleChanged != null && filteredAngle != -1)
				{
					NotifyAngleChanged(filteredAngle);
				}
			}
		}
	}
}