using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TaxiMobile.ListViewStructure;

namespace TaxiMobile.ListViewCell
{
	public abstract class InfoCell
	{
		private View _cellView;
//		
//		public InfoCell () : base()
//		{		
//		}
//
//		public InfoCell (IntPtr handle)
//		{		
//		}
		
		public InfoCell (SectionItem item, ViewGroup parent, Context context)
		{
			Parent = parent;
			OwnerContext = context;
			Item = item;
			
		}
		
		
		public ViewGroup Parent{ get; private set; }
		
		public Context OwnerContext{ get; private set; }
		
		public SectionItem Item{ get; private set; }
		
		public View CellView {
			get { return _cellView; }
			set { _cellView = value;
				//_cellView.Click -= CellSelected;
				//_cellView.Click += CellSelected;
			}
		}
		
		public virtual void Initialize()
		{
			
		}

		public virtual void CellSelect()
		{
			
		}

		public virtual void Dispose()
		{
			if( _cellView != null )
			{
				_cellView.Dispose();
				_cellView = null;
			}
		}
		
		private void CellSelected (object sender, EventArgs e)
		{
			CellSelect();
		}
		
		public virtual void LoadData()
		{}
	}
	
	public abstract class InfoCell<T> : InfoCell where T: SectionItem
	{
//		public InfoCell () : base()
//		{		
//		}
//
//		public InfoCell (IntPtr handle) : base(handle)
//		{		
//		}
//		
		public InfoCell (T item, ViewGroup parent, Context context) : base(item, parent, context )
		{
			
		}

		public T SectionItem {
			get{ return (T)base.Item;}
		}
	}
}

