using System;
using System.Collections.Generic;


namespace apcurium.MK.Common.Entity
{	
	public class BaseRateInfo
	{
        public decimal BaseFare { get; set; }
        public List<Tuple<string,decimal>> CustomFares { get; set; }
        public decimal BookingFeeCurrent { get; set; }
		public decimal BookingFeeAdvance { get; set; }
	}
}