using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("VehicleTypeSubView")]
    public class VehicleTypeSubView : UIControl
    {
        private const float LabelPadding = 10f;
        private UILabel _vehicleTypeLabel { get; set; }
        private UIView _borderView { get; set; }

        private UIColor DefaultColorForText = UIColor.FromRGB(153, 153, 153);
        private UIColor DefaultColorForBorder = UIColor.Clear;

        public VehicleTypeSubView (CGRect frame, VehicleType vehicle, bool isSelected) : base (frame)
        {
            Initialize();

            Vehicle = vehicle;
            Selected = isSelected;
        }

        private void Initialize()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            _vehicleTypeLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.FromName (FontName.HelveticaNeueBold, 18 / 2),
                TextColor = DefaultColorForText,
                ShadowColor = UIColor.Clear,
                LineBreakMode = UILineBreakMode.TailTruncation,
                TextAlignment = UITextAlignment.Center
            };
             
            AddSubview (_vehicleTypeLabel);

            // Constraints for VehicleTypeLabel
            AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(_vehicleTypeLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _vehicleTypeLabel.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(_vehicleTypeLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _vehicleTypeLabel.Superview, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(_vehicleTypeLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _vehicleTypeLabel.Superview, NSLayoutAttribute.CenterX, 1f, 0f)
            });

            _borderView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _borderView.Layer.BorderColor = UIColor.Clear.CGColor;
            _borderView.Layer.BorderWidth = 1f;
            _borderView.Layer.CornerRadius = 8f;

            _vehicleTypeLabel.AddSubview(_borderView);

            AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(_borderView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _borderView.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(_borderView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _borderView.Superview, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(_borderView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, _borderView.Superview, NSLayoutAttribute.Left, 1f, -LabelPadding),
                NSLayoutConstraint.Create(_borderView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, _borderView.Superview, NSLayoutAttribute.Right, 1f, LabelPadding)
            });
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

                    _vehicleTypeLabel.TextColor = DefaultColorForText;
                    _vehicleTypeLabel.Text = Localize.GetValue (value.Name.ToUpper ());
                    _vehicleTypeLabel.SizeToFit ();
                }
            }
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
                        _vehicleTypeLabel.TextColor = Theme.CompanyColor;
                        _borderView.Layer.BorderColor = Theme.CompanyColor.CGColor;
                    } 
                    else 
                    {
                        _vehicleTypeLabel.TextColor = DefaultColorForText;
                        _borderView.Layer.BorderColor = DefaultColorForBorder.CGColor;
                    }
                }
            }
        }
    }
    
}
