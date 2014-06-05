using System;
using CrossUI.Droid.Dialog.Elements;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Android.Content.Res;
using Android.Text;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Dialog
{
	public class TaxiHailEntryElement : EntryElement
	{
		public TaxiHailEntryElement (string caption, string hint, string value, string layoutName)
			:base(caption, hint, value, layoutName)
		{

		}

		protected override void UpdateDetailDisplay (Android.Views.View cell)
		{
			base.UpdateDetailDisplay (cell);
			if (cell != null) {
				//hard coded but since we are getting rid of dialog, no need to abstract this s**t
				var editText = cell.FindViewById<EditTextEntry> (Resource.Id.dialog_ValueField);
				editText.InputType = InputTypes.TextFlagCapWords;
			}
		}
	}
}

