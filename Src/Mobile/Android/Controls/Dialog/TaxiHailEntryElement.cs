﻿using System;
using CrossUI.Droid.Dialog.Elements;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Android.Content.Res;
using Android.Text;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Dialog
{
	public class TaxiHailEntryElement : EntryElement
	{
		InputTypes? _inputType;

		public TaxiHailEntryElement (string caption, string hint, string value, string layoutName, InputTypes? inputType = null)
			:base(caption, hint, value, layoutName)
		{
			_inputType = inputType;

		}

		protected override void UpdateDetailDisplay (Android.Views.View cell)
		{
			base.UpdateDetailDisplay (cell);
			if (cell != null) {
				//hard coded but since we are getting rid of dialog, no need to abstract this s**t
				var editText = cell.FindViewById<EditTextEntry> (Resource.Id.dialog_ValueField);

				if (_inputType != null) {
					editText.InputType = _inputType.Value;
				}

				if(this.Services().Localize.IsRightToLeft)
				{
					editText.Gravity = GravityFlags.Right 
										| GravityFlags.CenterVertical;
				}

			}
		}
	}
}

