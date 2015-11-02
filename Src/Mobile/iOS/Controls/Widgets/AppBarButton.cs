using UIKit;
using CoreGraphics;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class AppBarButton : CommandButton
    {
        private readonly UILabel _label;
        private readonly UIColor _selectedTextColor = UIColor.FromRGB(0, 126, 249);
        private readonly UIColor _regularTextColor = UIColor.FromRGB(41, 43, 45);
        private readonly UIColor _disabledTextColor = UIColor.FromRGB(151, 151, 151);

        public AppBarButton (string text, nfloat width, nfloat height, string imageName, string selectedImage = null, UIColor selectedTextColor = null) 
            : base(new CGRect(0, 0, width, height))
        {
            if (selectedTextColor != null)
            {
                _selectedTextColor = selectedTextColor;
            }

            var image = UIImage.FromFile(imageName);
            SetImage(image, UIControlState.Normal);
            if (selectedImage.HasValue())
            {
                SetImage(UIImage.FromFile(selectedImage), UIControlState.Selected);
                SetImage(UIImage.FromFile(selectedImage), UIControlState.Highlighted);
            }
            else
            {
                SetBackgroundImage(UIImage.FromFile("highlight.png"), UIControlState.Highlighted);
            }

            _label = new UILabel (new CGRect(0, height - 14, width, 22));
            _label.Text = text;
            _label.Font = UIFont.FromName(FontName.HelveticaNeueMedium, 20/2);
            _label.TextColor = _regularTextColor;
            _label.SizeToFit();
            _label.SetY(height - 14);
            _label.SetWidth(width);
            _label.BackgroundColor = UIColor.Clear;
            _label.TextAlignment = UITextAlignment.Center;
            AccessibilityLabel = text;

            AddSubviews ( _label);
        }

        public string Text 
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        public override bool Selected
        {
            get { return base.Selected; }
            set
            {
                base.Selected = value;
                _label.TextColor = Selected 
                    ? _selectedTextColor 
                    : _regularTextColor;
            }
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                _label.TextColor = Enabled 
                    ? _regularTextColor 
                    : _disabledTextColor;
            }
        }
    }
}