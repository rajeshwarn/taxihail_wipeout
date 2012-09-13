
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;


namespace apcurium.MK.Booking.Mobile.ListViewStructure
{
	public class SingleLineItem : SectionItem
	{
		public SingleLineItem ( string firstLine ) 
		{
			Label = firstLine;
			ShowRightArrow = true;
		}

		public bool ShowRightArrow { get; set; }

	}	
	
}

