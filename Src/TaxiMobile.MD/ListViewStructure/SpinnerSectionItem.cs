using System;
using System.Collections.Generic;

namespace TaxiMobile.ListViewStructure
{
	public class SpinnerSectionItem : SectionItem
	{
		private Func<int> _getValue;
		private Action<int> _setValue;
		private Func<List<ListItemData>> _getValues;
		
		public SpinnerSectionItem ( string label, Func<int> getValue, Action<int> setValue, Func<List<ListItemData>> getValues )
		{
			Label = label;	
			_getValue = getValue;
			_setValue = setValue;
			_getValues = getValues;
		}
		
		public Func<int> GetValue {
			get{ return _getValue; }
		}
		
		public Action<int> SetValue {
			get{ return _setValue; }
		}
		
		public Func<List<ListItemData>> GetValues {
			get{ return _getValues; }
		}
	}
}

