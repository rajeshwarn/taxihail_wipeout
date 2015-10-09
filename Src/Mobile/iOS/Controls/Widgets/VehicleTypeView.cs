using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Common.Extensions;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("VehicleTypeView")]
    public class VehicleTypeView : UIControl
    {
		private nfloat EstimateLabelMinimumWidth = 40f;
        private nfloat EstimateLabelPadding = 5f;
        private CGSize ImageSize = new CGSize(34f, 34f);

        private UIImageView _vehicleTypeImage { get; set; }
        private UILabel _vehicleTypeLabel { get; set; }
        private bool _isForEstimate { get; set; }

        public VehicleTypeView(CGRect frame) : base(frame)
        {
            _isForEstimate = true;

			Initialize();

            Selected = true;
        }

		public VehicleTypeView (CGRect frame, VehicleType vehicle, bool isSelected) : base (frame)
        {
            _isForEstimate = false;

            Initialize();

            Vehicle = vehicle;
            Selected = isSelected;
        }

        private void Initialize()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            _vehicleTypeImage = new UIImageView();
            _vehicleTypeImage.TranslatesAutoresizingMaskIntoConstraints = false;
            _vehicleTypeImage.IsAccessibilityElement = true;

			AddSubview (_vehicleTypeImage);

            // Constraints for VehicleTypeImage
            AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(_vehicleTypeImage, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ImageSize.Width),
                NSLayoutConstraint.Create(_vehicleTypeImage, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ImageSize.Height),
                NSLayoutConstraint.Create(_vehicleTypeImage, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _vehicleTypeImage.Superview, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(_vehicleTypeImage, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _vehicleTypeImage.Superview, NSLayoutAttribute.Top, 1f, 4f)
            });

            _vehicleTypeLabel = new UILabel 
            {
                BackgroundColor = UIColor.Clear,
                Font = UIFont.FromName (FontName.HelveticaNeueBold, 18 / 2),
                TextColor = DefaultColorForTextAndImage,
                ShadowColor = UIColor.Clear,
                LineBreakMode = UILineBreakMode.TailTruncation,
                TextAlignment = UITextAlignment.Center
            };

            _vehicleTypeLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview (_vehicleTypeLabel);

            // Constraints for VehicleTypeLabel
            AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(_vehicleTypeLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 12f),
                NSLayoutConstraint.Create(_vehicleTypeLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _vehicleTypeImage, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(_vehicleTypeLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, _vehicleTypeLabel.Superview, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(_vehicleTypeLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, _vehicleTypeLabel.Superview, NSLayoutAttribute.Right, 1f, 0f)
            });
        }

        public nfloat WidthToFitLabel()
        {
            if (!_vehicleTypeLabel.Text.HasValue())
            {
                return EstimateLabelMinimumWidth;
            }

            return NMath.Max(EstimateLabelMinimumWidth, this.GetSizeThatFits(_vehicleTypeLabel.Text, _vehicleTypeLabel.Font).Width) + EstimateLabelPadding * 2;
        }

        public override bool Selected 
        {
            get { return base.Selected; }
            set 
            {
                if (base.Selected != value)
                {
                    base.Selected = value;

                    if (Vehicle == null)
                    {
                        return;
                    }

                    if (value) 
                    {
						_vehicleTypeImage.Image = GetColoredImage(Vehicle.LogoName, Theme.CompanyColor);
                        _vehicleTypeLabel.TextColor = Theme.CompanyColor;
                    } 
                    else 
                    {
						_vehicleTypeImage.Image = GetColoredImage(Vehicle.LogoName, DefaultColorForTextAndImage);
                        _vehicleTypeLabel.TextColor = DefaultColorForTextAndImage;
                    }
                }
            }
        }
           
        private VehicleType _vehicle;
        public VehicleType Vehicle
        {
            get { return _vehicle; }
            set
            {
                if (_vehicle != value)
                {
                    _vehicle = value;

                    _vehicleTypeImage.Image = GetColoredImage (value.LogoName, Theme.LabelTextColor);
                    _vehicleTypeLabel.TextColor = DefaultColorForTextAndImage;
                    _vehicleTypeLabel.Text = Localize.GetValue (value.Name.ToUpper ());
                    _vehicleTypeImage.AccessibilityLabel = _vehicle.Name;
                    _vehicleTypeLabel.SizeToFit ();
                }
            }
        }

        private UIColor DefaultColorForTextAndImage
        {
            get 
            { 
                return _isForEstimate 
                    ? Theme.LabelTextColor
                    : UIColor.FromRGB(153, 153, 153);
            }
        }

		private UIImage GetColoredImage(string vehicleTypeLogoName, UIColor color)
        {			
			return ImageHelper.ApplyColorToImage(
				string.Format(Selected
					? "{0}_badge_selected.png" 
					: "{0}_badge.png", vehicleTypeLogoName.ToLower()), 
                color);
        }
    }
}
