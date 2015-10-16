using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("EtaView")]
    public class EtaView : UIControl
    {
    	private const float EtaLabelHeight = 23f;
		private const float BaseRateHeight = 100f;

		private const float AnimationFps = 30f;
		private const float AnimationVelocity = 4f;

        private CGSize ImageSize = new CGSize(34f, 34f);

		private BaseRateControl BaseRate { get; set; }
        private UIImageView EtaBadge { get; set; }
        private UILabel EtaLabel { get; set; }

		private bool _displayBaseRateInfo;
		public bool DisplayBaseRateInfo
		{ 
			get { return _displayBaseRateInfo; }
			set { _displayBaseRateInfo = value; Initialize(); }
		} 
		// Setting hardcoded to true for now.
        public bool BaseRateToggled { get; set; }

        public EtaView(IntPtr h) : base(h)
        {
            Initialize();
        }

        public EtaView ()
        {
            Initialize();
        }

        private void Initialize ()
		{
			new UIView[]{EtaLabel, EtaBadge, BaseRate}.ToList().ForEach(x => {
				if (x != null){
					x.RemoveFromSuperview();
				}
			});

			BackgroundColor = Theme.CompanyColor;
			TranslatesAutoresizingMaskIntoConstraints = false;

			EtaLabel = new UILabel {
				AdjustsFontSizeToFitWidth = true,
				BackgroundColor = UIColor.Clear,
				Lines = 1,
				Font = UIFont.FromName (FontName.HelveticaNeueLight, 30 / 2),
				TextAlignment = UITextAlignment.Center,
				TextColor = Theme.LabelTextColor,
				ShadowColor = UIColor.Clear,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			if (!DisplayBaseRateInfo)
			{
				EtaBadge = new UIImageView ();
				EtaBadge.TranslatesAutoresizingMaskIntoConstraints = false;
				AddSubviews (EtaLabel, EtaBadge);

				// Constraints for EtaBadge
				AddConstraints (new [] {
					NSLayoutConstraint.Create (EtaBadge, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ImageSize.Width),
					NSLayoutConstraint.Create (EtaBadge, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, ImageSize.Height),
					NSLayoutConstraint.Create (EtaBadge, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EtaBadge.Superview, NSLayoutAttribute.Left, 1f, 4f),
					NSLayoutConstraint.Create (EtaBadge, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, EtaBadge.Superview, NSLayoutAttribute.CenterY, 1f, 0f),
				});

				// Constraints for EtaLabel
				AddConstraints (new [] {
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Height, 1f, 0f),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EtaBadge, NSLayoutAttribute.Right, 1f, 4f),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Right, 1f, -4f),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.CenterY, 1f, 0f),
				});
			} else
			{
				BaseRate = new BaseRateControl ();
				AddSubviews(BaseRate);
				BaseRate.AddSubview(EtaLabel);

				// Constraints for EtaLabel
				BaseRate.AddConstraints (new [] {
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Height, 1f, EtaLabelHeight),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Left, 1f, 4f),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Right, 1f, -4f),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Bottom, 1f, EtaLabelHeight),
				});

				AddConstraints (new [] {
					NSLayoutConstraint.Create (BaseRate, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, BaseRate.Superview, NSLayoutAttribute.Bottom, 1f, -EtaLabelHeight),
					NSLayoutConstraint.Create (BaseRate, NSLayoutAttribute.Left, NSLayoutRelation.Equal, BaseRate.Superview, NSLayoutAttribute.Left, 1f, 0f),
					NSLayoutConstraint.Create (BaseRate, NSLayoutAttribute.Right, NSLayoutRelation.Equal, BaseRate.Superview, NSLayoutAttribute.Right, 1f, 0f),
					NSLayoutConstraint.Create (BaseRate, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, BaseRateHeight)
				});

				TouchUpInside += (object sender, EventArgs e) => ToggleBaseRate();
				BaseRate.TouchUpInside += (object sender, EventArgs e) => ToggleBaseRate();
			}
		}

		public void ToggleBaseRate ()
		{
			BaseRateToggled = !BaseRateToggled;

			if (BaseRate.BaseRate == null)
			{
				BaseRateToggled = false;
			}

			var orderOptionsControl = ((OrderOptionsControl)Superview.Superview);
			var orderOptionsHeightWithoutEta = 
				(nfloat)orderOptionsControl.Subviews [0]
				.Subviews
				.Where (x => !x.Hidden)
				.Sum (x => x.Frame.Height) - Frame.Height;

			var newEtaViewHeight = (BaseRateToggled ? BaseRateHeight : 0f) + EtaLabelHeight;
			var height = Constraints.FirstOrDefault (x => x.FirstItem == this && x.FirstAttribute == NSLayoutAttribute.Height);
			float iterations = 0;
			float delta = newEtaViewHeight - (float)height.Constant;
			float incrementation = (delta / AnimationFps) * AnimationVelocity;
			var originalToggleState = BaseRateToggled;

			SetNeedsLayout();
			orderOptionsControl.SetNeedsLayout();

			var layoutViews = new Action(() => {
				LayoutIfNeeded ();
				orderOptionsControl.LayoutIfNeeded ();
			});

			NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromMilliseconds (1000d / AnimationFps), _ => {
				iterations += AnimationVelocity;
				height.Constant += incrementation;
				orderOptionsControl.HeightConstraint.Constant = orderOptionsHeightWithoutEta + height.Constant;
				if (originalToggleState != BaseRateToggled)
				{
					layoutViews.Invoke();
					_.Invalidate ();
				}
				if (iterations >= AnimationFps)
				{
					height.Constant = newEtaViewHeight;
					orderOptionsControl.HeightConstraint.Constant = orderOptionsHeightWithoutEta + newEtaViewHeight;
					layoutViews.Invoke();
					_.Invalidate ();
				}
				layoutViews.Invoke();
			});
		}

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
			this.SetRoundedCorners(UIRectCorner.BottomLeft | UIRectCorner.BottomRight, 3f);
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
				}

				if (!DisplayBaseRateInfo)
				{
					EtaBadge.Image = ImageHelper.ApplyColorToImage (string.Format ("{0}_no_badge_selected.png", value.LogoName.ToLower ()), Theme.LabelTextColor);
				} else
				{
					BaseRate.BaseRate = value.BaseRate;
					if (value.BaseRate == null && BaseRateToggled)
					{
						ToggleBaseRate();
					}
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
                            ? this.Superview.Constraints.Where (x => x.FirstItem == this || x.SecondItem == this).ToArray ()
                            : null;
						if (_hiddenContraints != null)
						{
							this.Superview.RemoveConstraints (_hiddenContraints);
						}
					} else
					{
						if (_hiddenContraints != null)
						{
							this.Superview.AddConstraints (_hiddenContraints);
							_hiddenContraints = null;
						}
					}

					if (Superview != null)
					{
						((OrderOptionsControl)Superview.Superview).Resize ();
					}
				}
			}
		}
    }
}
