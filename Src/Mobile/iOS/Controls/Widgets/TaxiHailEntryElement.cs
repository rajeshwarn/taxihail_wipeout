using CrossUI.Touch.Dialog.Elements;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class TaxiHailEntryElement : EntryElement
	{
        private float Padding = 13.5f;
		private bool _isPassword;
		private UITextAutocapitalizationType _autocapitalizationType;

		public TaxiHailEntryElement(string caption, string placeholder, string value = "", bool isPassword = false, UITextAutocapitalizationType autocapitalizationType = UITextAutocapitalizationType.None)
			: base(caption, placeholder, value, isPassword)
		{
			_autocapitalizationType = autocapitalizationType;
			_isPassword = isPassword;
		}

		protected override UITextField CreateTextField(CGRect frame)
		{
			UITextField textField;
			if (UIHelper.IsOS7orHigher)
			{
                textField  = base.CreateTextField(frame.SetX(Padding).SetWidth(UIScreen.MainScreen.Bounds.Width - 2*8 - 2*Padding)); // screenwidth - margin - padding
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
            textField.AdjustsFontSizeToFitWidth = true;
            textField.AccessibilityLabel = Placeholder;

			if (_isPassword) 
            {
				this.AutocapitalizationType = UITextAutocapitalizationType.None;
			}
			else 
            {
				this.AutocapitalizationType = _autocapitalizationType;
			}

			return textField;
		}
	}
}

