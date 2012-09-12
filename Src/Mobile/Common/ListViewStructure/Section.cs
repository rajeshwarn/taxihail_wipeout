using System;
using System.Linq;
using System.Collections.Generic;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ListViewStructure
{
	public class Section :IDisposable
	{
		private List<SectionItem> _items;
		private  float? _rowHeight;
		
		public Section (InfoStructure parent)
		{
			_items = new List<SectionItem> ();
			Parent = parent;
			SectionLabelTextColor = new float[] { 0f, 0f, 0f, 1f };
		}
		
		public InfoStructure Parent {
			get;
			private set;
		}

		public void Clear ()
		{
			_items = new List<SectionItem>();;
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

		public float[] SectionLabelTextColor {
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
			item.Parent.Parent.DoItemDeleted ( item );
			item.Parent = null;
			_items.Remove (item);	
		}
		
		public Action<SectionItem> OnItemDeleted {
			get; 
			set;
		}

		public bool EditMode { get; set; }
		
		#region IDisposable implementation
		public void Dispose ()
		{
			Items.ForEach ( i=> i.Dispose ());
			_items = null;
			Parent = null;			
			OnItemDeleted= null;
		}
		#endregion
	}
}

