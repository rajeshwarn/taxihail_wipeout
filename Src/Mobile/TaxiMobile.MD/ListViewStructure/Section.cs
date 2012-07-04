using System;
using System.Collections.Generic;

namespace TaxiMobile.ListViewStructure
{
	public class Section
	{
		private List<SectionItem> _items;
		private  float? _rowHeight;
		
		public Section (ListStructure parent)
		{
			_items = new List<SectionItem> ();
			Parent = parent;
		}
		
		public ListStructure Parent {
			get;
			private set;
		}
			
		public float RowHeight {
			get {
				if (_rowHeight.HasValue)
				{
					return _rowHeight.Value;
				}
				else
				{
					return Parent.RowHeight;
				}
			}	
			set {
				_rowHeight = value;
			}
		}
		
		public IEnumerable<SectionItem> Items {
			get { return _items;}		
		}
		
		public string SectionLabel {
			get;
			set;
		}
		
		public void AddItem (SectionItem item)
		{
			item.Parent = this;
			_items.Add (item);
		}

		public void RemoveItem (SectionItem item)
		{
			if (OnItemDeleted != null)
			{
				OnItemDeleted (item);
			}
			item.Parent = null;
			_items.Remove (item);	
		}
		
		public Action<SectionItem> OnItemDeleted {
			get; 
			set;
		}
		
		
	}
}

