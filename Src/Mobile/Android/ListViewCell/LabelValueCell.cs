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
	public abstract class LabelValueCell<T> : InfoCell<T>  where T: SectionItem
	{
	
//		public LabelValueCell () : base()
//		{		
//		}
//
//		public LabelValueCell (IntPtr handle) : base(handle)
//		{		
//		}
//		
		public LabelValueCell (T item, ViewGroup parent, Context context) : base( item, parent, context )
		{
		}
		
		public TextView Label {
			get;
			set;
		}

		public EditText Value {
			get;
			set;
		}

		public override void Initialize ()
		{
			LayoutInflater inflater = (LayoutInflater) OwnerContext.GetSystemService( Context.LayoutInflaterService );
		 	CellView = inflater.Inflate( Resource.Layout.LabelValueCell, Parent, false );
			Label = (TextView) CellView.FindViewById( Resource.Id.label );
			Value = (EditText) CellView.FindViewById( Resource.Id.value );
		}

		public override void LoadData ()
		{

		}
		
		public override void CellSelect ()
		{
			
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			if( Label != null )
			{
				Label.Dispose();
				Label = null;
			}
			if( Value != null )
			{
				Value.Dispose();
				Value = null;
			}
		}
	
	}
	
	
	
	
}

