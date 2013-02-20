using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class ListItemData
	{
		private static ListItemData _nullListItemData;
		public static ListItemData NullListItemData {
			get {
				if(_nullListItemData == null)
				{
					var resources = TinyIoCContainer.Current.Resolve<IAppResource>();
					_nullListItemData = new ListItemData
					{
						Key = int.MinValue,
						Value = resources.GetString("NoPreference")
					};
				}
				return _nullListItemData;
			}
		}
		
		public string Value {
			get;
			set;
		}
		
		public int Key {
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

