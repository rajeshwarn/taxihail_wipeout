using System;
using CrossUI.Touch.Dialog.Elements;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class TaxiHailEntryElement : EntryElement
	{
		private bool _isPassword;

		public TaxiHailEntryElement(string caption, string placeholder, string value = "", bool isPassword = false)
			: base(caption, placeholder, value, isPassword)
		{
			_isPassword = isPassword;
		}

		protected override UITextField CreateTextField(RectangleF frame)
		{
			UITextField textField;
			if (UIHelper.IsOS7orHigher)
			{
				textField  = base.CreateTextField(frame.SetX(20).IncrementWidth(-30));
			}
			else
			{
				textField  = base.CreateTextField(frame);
			}

			textField.VerticalAlignment = UIControlContentVerticalAlignment.Center;

			if (_isPassword)
			{
				this.AutocorrectionType = UITextAutocorrectionType.No;
				this.AutocapitalizationType = UITextAutocapitalizationType.None;
			}

			return textField;
		}
	}
}

