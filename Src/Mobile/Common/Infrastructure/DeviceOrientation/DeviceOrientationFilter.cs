using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation
{
    public class DeviceOrientationFilter
    {
        private struct FilterValue
        {
            public int Value;
            public long Timestamp;
        }

        private const int BufferCapacity = 1000;
        private const int MaximumRandomDeviation = 2;

        private readonly FilterValue[] _buffer = new FilterValue[BufferCapacity];
        private int _bufferTopPointer = -1;
        private int _bufferLength;
        private int _exclusiveAccess;

        public void AddValue(int value, long time)
        {
            if (System.Threading.Interlocked.CompareExchange(ref _exclusiveAccess, 1, 0) == 0)
            {
                _bufferTopPointer++;

                if (_bufferTopPointer == BufferCapacity)
                {
                    _bufferTopPointer = 0;
                }

                _buffer[_bufferTopPointer].Value = value;
                _buffer[_bufferTopPointer].Timestamp = time;

                if (_bufferLength < BufferCapacity)
                {
                    _bufferLength++;
                }

                _exclusiveAccess = 0;
            }
        }

        FilterValue ReadValueFromEnd(int position)
        {
            var result = new FilterValue();

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
		/// Takes the events of the last [timeIntervalToGatherEventsSet] milliseconds or, if this time is not be able to gather, the last 10 events,
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

                    var fv1 = ReadValueFromEnd(0);
                    var fv2 = ReadValueFromEnd(0);

                    int maximumValueDifference = 0;

                    for (int i = 0; i < _bufferLength; i++)
                    {
                        fv2 = ReadValueFromEnd(i);

                        time += fv1.Timestamp - fv2.Timestamp;

                        int valueDifference = Math.Abs(fv2.Value - fv1.Value);

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

					if (timeAchieved && maximumValueDifference <= MaximumRandomDeviation)
                    {
                        result = ReadValueFromEnd(0).Value;
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
}
