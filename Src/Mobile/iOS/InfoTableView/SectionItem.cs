using System;
using System.Collections.Generic;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public abstract class SectionItem : IDisposable
	{
		protected  float? _rowHeight;
		private Dictionary<string,object> _uiState = null;
		
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
		
		public void SetUIState (string key, object value)
		{
			if (_uiState == null)
			{
				_uiState = new Dictionary<string, object> ();
			}
			
			if (_uiState.ContainsKey (key))
			{
				_uiState [key] = value;				
			}
			else
			{
				_uiState.Add (key, value);	
			}
		}
		
		public T GetUIState<T> (string key)
		{
			if (_uiState == null)
			{
				return default( T );
			}			
			else if ( (_uiState.ContainsKey (key) ) &&  (_uiState [key] is T ) )
			{
				return (T) _uiState [key];	
			}
			else
			{
				return default( T );
			}
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

		public int Index {
			get {
				return Parent.Items.IndexOf( this, (item1, item2) => item1.Data == item2.Data );
			}
		}
		
		public Guid? Id { get; set; }
		
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
		
		public virtual void Dispose ()
		{
			Parent = null;
			Enabled = null;
			Data = null;
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

