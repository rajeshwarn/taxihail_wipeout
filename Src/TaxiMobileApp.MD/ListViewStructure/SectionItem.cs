using System;

namespace TaxiMobile.ListViewStructure
{
	public abstract class SectionItem
	{
		protected  float? _rowHeight;
		
		public SectionItem (Section parent)
		{			
			Parent = parent;
			Enabled = () => true;
			CanDelete = false;
		}
		
		public SectionItem ()
		{			
			Enabled = () => true;
			CanDelete = false;
		}
		
		public Section Parent {
			get;
			set;
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
		
		public string Label {
			get;
			set;
		}
		
		
		//public virtual string Identifier{get{return null;}}
		
		public Guid? Id { get; set; }
		
		public int? Key { get; set; }
		
		public object Data { get; set; }
		
		public bool Highlighted {
			get;
			set;
		}
		
		public TextAlignment LabelAlignment {
			get;
			set;
		}
		
		public TextWidth LabelWidth {
			get;
			set;
		}
		
		public bool CanDelete { get; set; }
		
		public Func<bool> Enabled { get; set; }
		
		public virtual bool Commit ()
		{
			return true;
		}
		
	}
	
	public enum TextAlignment
	{
		AlignLeft,
		AlignCenter,
		AlignRight,
	}
	
	public enum TextWidth
	{
		Small,
		Medium,
		Large,
	}
	
	
	
}

