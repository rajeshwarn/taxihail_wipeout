using System;

namespace TaxiMobile.ListViewStructure
{
	public class BooleanSectionItem : SectionItem
	{
		private Func<BooleanSectionItem, bool> _getValue;
		private Action<int> _setValue;
		
		public BooleanSectionItem ( int key, string label, Func<BooleanSectionItem, bool> getValue, Action<int> setValue )
		{
			Label = label;	
			Key = key;
			_getValue = getValue;
			_setValue = setValue;
		}
		
		public Func<BooleanSectionItem, bool> GetValue {
			get{ return _getValue; }
		}
		
		public Action<int> SetValue {
			get{ return _setValue; }
		}

	}
}

