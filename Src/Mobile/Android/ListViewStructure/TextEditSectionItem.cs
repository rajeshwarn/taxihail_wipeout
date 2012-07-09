using System;

namespace TaxiMobile.ListViewStructure
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
		
		public Func<string> GetValue {
			get{ return _getValue; }
		}
		
		public Action<string> SetValue {
			get{ return _setValue; }
		}
			
	}
}

