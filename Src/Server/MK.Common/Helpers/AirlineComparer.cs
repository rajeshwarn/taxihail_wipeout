using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Helpers
{
	public class AirlineComparer : IEqualityComparer<Airline>
	{
		public bool Equals(Airline x, Airline y)
		{
			return x.Id.Equals(y.Id) && x.Name.Equals(y.Name);
		}

		public int GetHashCode(Airline obj)
		{
			return obj.Id.GetHashCode() ^ obj.Name.GetHashCode();
		}
	}
}