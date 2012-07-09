using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using TaxiMobile.ListViewStructure;

namespace TaxiMobile.ListViewCell
{
	class BooleanCell : LabelValueCell<BooleanSectionItem>
	{
		
		public BooleanCell (BooleanSectionItem item, ViewGroup parent, Context context) : base( item, parent, context )
		{
			
		}
		
		public override void Initialize ()
		{
			LayoutInflater inflater = (LayoutInflater) OwnerContext.GetSystemService( Context.LayoutInflaterService );
		 	CellView = inflater.Inflate( Resource.Layout.BooleanCell, Parent, false );
			RadioBtn = (CheckedTextView) CellView.FindViewById( Resource.Id.radioButton );
			RadioBtn.Touch -= HandleRadioBtnTouch;
			RadioBtn.Touch += HandleRadioBtnTouch;
		}

		void HandleRadioBtnTouch (object sender, View.TouchEventArgs e)
		{
			SectionItem.SetValue( SectionItem.Key.Value );
			((ListViewAdapter)((ListActivity)OwnerContext).ListAdapter).NotifyDataSetChanged();
		}

		public CheckedTextView RadioBtn { get; set; }
		
		public override void LoadData ()
		{
			RadioBtn.Text = SectionItem.Label;
			RadioBtn.Checked = SectionItem.GetValue(SectionItem);
		}

	}
}

