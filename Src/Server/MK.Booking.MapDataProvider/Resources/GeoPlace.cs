using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.MapDataProvider.Resources
{
	public class GeoPlace
	{
		public GeoPlace ()
		{
			Address = new GeoAddress ();
		}

		public string Id {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public IEnumerable<string> Types {
			get;
			set;
		}

		public GeoAddress Address {
			get;
			set;
		}
	}
}

