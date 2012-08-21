
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;


namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	
	public class TwoLinesAddressItem : SectionItem
	{
		public TwoLinesAddressItem ( Guid id, string firstLine, string secondLine ) 
		{
			Id = id;
			Label = firstLine;
			DetailText = secondLine;
			ShowRightArrow = true;
		}

		public bool ShowRightArrow { get; set; }
		public bool ShowPlusSign { get; set; }
		public string DetailText { get; set; }


	}
	
	
	
}

