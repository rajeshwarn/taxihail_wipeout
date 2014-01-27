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
        private float Padding = 13.5f;
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
                textField  = base.CreateTextField(frame.SetX(Padding).SetWidth(320 - 2*8 - 2*Padding)); // 320 - margin - padding
				textField.TintColor = UIColor.Black; // cursor color
			}
			else
			{
                textField  = base.CreateTextField(frame.SetX(Padding).IncrementY(-11).SetHeight(21));
			}

			textField.TextColor = UIColor.FromRGB(44, 44, 44);
			textField.Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);
			textField.VerticalAlignment = UIControlContentVerticalAlignment.Center;

			this.AutocorrectionType = UITextAutocorrectionType.No;

			if (_isPassword)
			{
				this.AutocapitalizationType = UITextAutocapitalizationType.None;
			}

			return textField;
		}
	}
}

