using System;

namespace apcurium.MK.Booking.Mobile.Client.ListViewStructure
{
	public class TextEditSectionItem : SectionItem
	{
		private Func<string> _getValue;
		private Action<string> _setValue;
		
		public TextEditSectionItem ( string label, Func<string> getValue, Action<string> setValue )
		{
			Label = label;	
			_getValue = getValue;
			_setValue = setValue;
		}

		public TextEditSectionItem ( string label, Func<int> getValue, Action<int> setValue )
		{
			Label = label;	
			_getValue = () => getValue().ToString();
			_setValue = x => {
				int value = 0;
				int.TryParse(x, out value);
				setValue(value);
			};
		}
		
		public Func<string> GetValue {
			get{ return _getValue; }
		}
		
		public Action<string> SetValue {
			get{ return _setValue; }
		}
			
	}
}

