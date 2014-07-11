using System;

namespace apcurium.MK.Booking.MapDataProvider.Resources
{
	public class GeoDirection
	{
		public GeoDirection ()
		{
		}

        /// <summary>
        /// Total distance in meters
        /// </summary>
        public int? Distance { get; set; }

        /// <summary>
        /// Total duration in seconds
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Traffic delay in seconds
        /// </summary>
        public int? TrafficDelay { get; set; }
	}
}

