//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Android.Graphics;
//using apcurium.MK.Booking.Mobile.Client.ListViewStructure;

//namespace apcurium.MK.Booking.Mobile.Client.ListViewCell
//{
//    class BooleanCell : LabelValueCell<BooleanSectionItem>
//    {
		
//        public BooleanCell (BooleanSectionItem item, ViewGroup parent, Context context) : base( item, parent, context )
//        {
			
//        }
		
//        public override void Initialize ()
//        {
//            LayoutInflater inflater = (LayoutInflater) OwnerContext.GetSystemService( Context.LayoutInflaterService );
//            CellView = inflater.Inflate( Resource.Layout.BooleanCell, Parent, false );
//            RadioBtn = (CheckedTextView) CellView.FindViewById( Resource.Id.radioButton );
//            RadioBtn.Touch -= HandleRadioBtnTouch;
//            RadioBtn.Touch += HandleRadioBtnTouch;
//        }

//        void HandleRadioBtnTouch (object sender, View.TouchEventArgs e)
//        {
//            SectionItem.SetValue( SectionItem.Key.Value );
//            ((ListViewAdapter)((ListActivity)OwnerContext).ListAdapter).NotifyDataSetChanged();
//        }

//        public CheckedTextView RadioBtn { get; set; }
		
//        public override void LoadData ()
//        {
//            RadioBtn.Text = SectionItem.Label;
//            RadioBtn.Checked = SectionItem.GetValue(SectionItem);
//        }

//    }
//}

