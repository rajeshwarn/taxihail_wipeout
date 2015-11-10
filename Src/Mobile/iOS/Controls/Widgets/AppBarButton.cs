using UIKit;
using CoreGraphics;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("AppBarButton")]
    public class AppBarButton : CommandButton
    {
        private UILabel _label;
		private UIColor _selectedTextColor;
		private UIColor _regularTextColor;

		public AppBarButton(IntPtr handle):base(handle)
		{
		}

		public AppBarButton(CGRect frame):base(frame)
		{
		}

		public AppBarButton():base()
		{
		}

		public void Initialize(string text, string imageName, string selectedImageName = null, UIColor selectedTextColor = null)
		{
			_regularTextColor = UIColor.FromRGB(41, 43, 45);
			_selectedTextColor = selectedTextColor ?? UIColor.FromRGB(0, 126, 249);

			var image = UIImage.FromFile(imageName);
			SetImage(image, UIControlState.Normal);
			if (selectedImageName.HasValue())
			{
				var selectedImage = UIImage.FromFile(selectedImageName);
				SetImage(selectedImage, UIControlState.Selected);
				SetImage(selectedImage, UIControlState.Highlighted);
			}
			else
			{
				SetBackgroundImage(UIImage.FromFile("highlight.png"), UIControlState.Highlighted);
			}

			_label = new UILabel (new CGRect(0, Frame.Height - 14, Frame.Width, 22));
			_label.Text = text;
			_label.Font = UIFont.FromName(FontName.HelveticaNeueMedium, 20/2);
			_label.TextColor = _regularTextColor;
			_label.SizeToFit();
			_label.SetY(Frame.Height - 14);
			_label.SetWidth(Frame.Width);
			_label.BackgroundColor = UIColor.Clear;
			_label.TextAlignment = UITextAlignment.Center;
			AccessibilityLabel = text;

			AddSubviews ( _label);
		}

        public string Text 
        {
            get 
            {
                return _label.Text;
            }
            set
            {
                _label.Text = value;
            }
        }

        public override bool Selected
        {
            get
            {
                return base.Selected;
            }
            set
            {
                base.Selected = value;
                if (Selected)
                {
                    _label.TextColor = _selectedTextColor;
                }
                else
                {
                    _label.TextColor = _regularTextColor;
                }
            }
        }
    }
}

