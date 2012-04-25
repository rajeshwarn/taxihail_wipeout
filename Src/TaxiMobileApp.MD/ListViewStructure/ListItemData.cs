using System;

namespace TaxiMobile
{
	public class ListItemData
	{
		public ListItemData ()
		{
		}
		
		public string Value {
			get;
			set;
		}
		
		public int Key {
			get;
			set;
		}
		public override string ToString ()
		{
			return Value;
		}

	}

}

