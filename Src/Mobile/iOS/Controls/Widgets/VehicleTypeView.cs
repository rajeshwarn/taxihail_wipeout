using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using apcurium.MK.Common.Extensions;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using MonoTouch.CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Localization;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("VehicleTypeView")]
    public class VehicleTypeView : UIControl
    {
        private UIImageView VehicleTypeImage { get; set; }
        private UILabel VehicleTypeLabel { get; set; }
        private bool IsForSelection { get; set; }
		private bool EtaBadge { get; set; }

        public VehicleTypeView(RectangleF frame) : base(frame)
        {
			Subviews.ForEach (x => x.RemoveFromSuperview ());
			Initialize();

            Selected = true;
        }

		public VehicleTypeView (RectangleF frame, VehicleType vehicle, bool isSelected, bool etaBadge = false) : base (frame)
        {
			EtaBadge = etaBadge;
			Initialize();
			IsForSelection = true;
            Vehicle = vehicle;
            Selected = isSelected;
        }

        private void Initialize()
        {
            VehicleTypeImage = new UIImageView(new RectangleF(this.Frame.Width / 2 - 34f / 2, 4f, 34f, 34f));

			AddSubview (VehicleTypeImage);

			VehicleTypeLabel = new UILabel {
				BackgroundColor = UIColor.Clear,
				Font = UIFont.FromName (FontName.HelveticaNeueBold, 18 / 2),
				TextColor = DefaultColorForTextAndImage,
				ShadowColor = UIColor.Clear,
				LineBreakMode = UILineBreakMode.TailTruncation,
				TextAlignment = UITextAlignment.Center
			};

			if (!EtaBadge) {
				AddSubview (VehicleTypeLabel);
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
						VehicleTypeImage.Image = GetColoredImage(Vehicle.LogoName, Theme.CompanyColor, EtaBadge);
                        VehicleTypeLabel.TextColor = Theme.CompanyColor;
                    } 
                    else 
                    {
						VehicleTypeImage.Image = GetColoredImage(Vehicle.LogoName, DefaultColorForTextAndImage, EtaBadge);
                        VehicleTypeLabel.TextColor = DefaultColorForTextAndImage;
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

					VehicleTypeImage.Image = GetColoredImage (value.LogoName, Theme.LabelTextColor, EtaBadge);
					if (!EtaBadge) {
						VehicleTypeLabel.TextColor = DefaultColorForTextAndImage;
						VehicleTypeLabel.Text = Localize.GetValue (value.Name.ToUpper ());
						VehicleTypeLabel.SizeToFit ();
						VehicleTypeLabel.SetWidth (this.Frame.Width);
						VehicleTypeLabel.SetY (VehicleTypeImage.Frame.Bottom);
					}
                }
            }
        }

        private UIColor DefaultColorForTextAndImage
        {
            get 
            { 
                return IsForSelection 
                    ? UIColor.FromRGB(153, 153, 153)
                    : Theme.LabelTextColor;
            }
        }

		public UIImage GetColoredImage(string vehicleTypeLogoName, UIColor color, bool etaBadge = false)
        {			
			return ImageHelper.ApplyColorToImage(
				string.Format(Selected
					? "{0}_" + (etaBadge ? "no_" : "") + "badge_selected.png" 
					: "{0}_" + (etaBadge ? "no_" : "") + "badge.png", 
                    vehicleTypeLogoName.ToLower()), 
				etaBadge 
                    ? Theme.LabelTextColor 
                    : color);
        }
    }
}
