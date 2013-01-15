using System;

namespace apcurium.MK.Booking.Mobile.Client
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
		
		public int? Key {
			get;
			set;
		}

		public string Image {
			get;
			set;
		}

		public override string ToString ()
		{
			return Value;
		}

	}

}

