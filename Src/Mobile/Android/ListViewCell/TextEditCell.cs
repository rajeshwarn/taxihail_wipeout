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
using Android.Graphics;
using TaxiMobile.ListViewStructure;

namespace TaxiMobile.ListViewCell
{
	class TextEditCell : LabelValueCell<TextEditSectionItem>
	{
		public TextEditCell (TextEditSectionItem item, ViewGroup parent, Context context) : base( item, parent, context )
		{
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			Value.TextChanged -= HandleValueTextChanged;
			Value.TextChanged += HandleValueTextChanged;
		}
		
		
		
		public override void LoadData ()
		{
			base.LoadData ();
			Label.Text = SectionItem.Label;
			Value.Text = SectionItem.GetValue();
		}
		
		void HandleValueTextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			SectionItem.SetValue( e.Text.ToString() );
		}
		
		
		public override void Dispose ()
		{
			base.Dispose ();
		}
	}
}

