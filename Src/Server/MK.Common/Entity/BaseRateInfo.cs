using System;
using System.Collections.Generic;


namespace apcurium.MK.Common.Entity
{	
	public class BaseRateInfo
	{
        public decimal MinimumFare { get; set; }
        public decimal BaseRateNoMiles { get; set; }
        public decimal PerMileRate { get; set; }
		public decimal WaitTime { get; set; }
	}
}