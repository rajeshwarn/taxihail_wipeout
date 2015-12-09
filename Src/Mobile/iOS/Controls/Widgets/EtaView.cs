using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Helper;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("EtaView")]
    public class EtaView : UIView
    {
        private const float EtaContainerHeight = 23f;
        private CGSize ImageSize = new CGSize(34f, 34f);

        private UIImageView EtaBadge { get; set; }
        private UILabel EtaLabel { get; set; }

        public EtaView(IntPtr h) : base(h)
        {
            Initialize();
        }

        public EtaView ()
        {
            Initialize();
        }

        private void Initialize()
        {
            EtaBadge = new UIImageView();
            EtaBadge.TranslatesAutoresizingMaskIntoConstraints = false;

            EtaLabel = new UILabel
            {
                AdjustsFontSizeToFitWidth = true,
                BackgroundColor = UIColor.Clear,
                Lines = 1,
                Font = UIFont.FromName(FontName.HelveticaNeueLight, 30 / 2),
                TextAlignment = UITextAlignment.Center,
                TextColor = Theme.LabelTextColor,
                ShadowColor = UIColor.Clear,
            };
            EtaLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            AddSubviews(EtaBadge, EtaLabel);

            // Constraints for EtaBadge
            AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(EtaBadge, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ImageSize.Width),
                NSLayoutConstraint.Create(EtaBadge, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ImageSize.Height),
                NSLayoutConstraint.Create(EtaBadge, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EtaBadge.Superview, NSLayoutAttribute.Left, 1f, 4f),
                NSLayoutConstraint.Create(EtaBadge, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, EtaBadge.Superview, NSLayoutAttribute.CenterY, 1f, 0f),
            });

            // Constraints for EtaLabel
            AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(EtaLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Height, 1f, 0f),
                NSLayoutConstraint.Create(EtaLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EtaBadge, NSLayoutAttribute.Right, 1f, 4f),
                NSLayoutConstraint.Create(EtaLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Right, 1f, -4f),
                NSLayoutConstraint.Create(EtaLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.CenterY, 1f, 0f),
            });
        }

        private VehicleType _selectedVehicle;
        public VehicleType SelectedVehicle
        {
            get { return _selectedVehicle; }
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;

                    EtaBadge.Image = ImageHelper.ApplyColorToImage(string.Format("{0}_no_badge_selected.png", value.LogoName.ToLower()), Theme.LabelTextColor);
                }
            }
        }

        public string Eta
        {
            get { return EtaLabel.Text; }
            set
            {
                if (EtaLabel.Text != value)
                {
                    EtaLabel.Text = value;
                }
            }
        }

        private NSLayoutConstraint[] _hiddenContraints { get; set; }
        public override bool Hidden
        {
            get
            {
                return base.Hidden;
            }
            set
            {
                if (base.Hidden != value)
                {
                    base.Hidden = value;
                    if (value)
                    {
                        _hiddenContraints = this.Superview.Constraints != null 
                            ? this.Superview.Constraints.Where(x => x.FirstItem == this || x.SecondItem == this).ToArray()
                            : null;
                        if (_hiddenContraints != null)
                        {
                            this.Superview.RemoveConstraints(_hiddenContraints);
                        }
                    }
                    else
                    {
                        if (_hiddenContraints != null)
                        {
                            this.Superview.AddConstraints(_hiddenContraints);
                            _hiddenContraints = null;
                        }
                    }

                    if (Superview != null)
                    {
                        ((OrderOptionsControl)Superview.Superview).Resize();
                    }
                }
            }
        }
    }
}
