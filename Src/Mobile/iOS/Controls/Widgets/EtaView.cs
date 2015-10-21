using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using CoreGraphics;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Helper;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("EtaView")]
    public class EtaView : UIView
    {
    	private const float EtaLabelHeight = 23f;
		private const float BaseRateHeight = 100f;

        private CGSize ImageSize = new CGSize(34f, 34f);

        private UIControl _fixedLowerBarView { get; set; }
		private BaseRateControl BaseRate { get; set; }
        private UIImageView EtaBadge { get; set; }
        private UILabel EtaLabel { get; set; }
        private bool BaseRateToggled { get; set; }

        // Setting hardcoded to true for now.
        public bool DisplayBaseRateInfo { get; set; }

        public bool UserInputDisabled
        {
            get { return !_fixedLowerBarView.UserInteractionEnabled; }
            set 
            {
                _fixedLowerBarView.UserInteractionEnabled = !value;
                if (!_fixedLowerBarView.UserInteractionEnabled && BaseRateToggled)
                {
                    // close the rate box
                    ToggleBaseRate();
                }
            }
        }

        private NSLayoutConstraint _rateBoxHeightConstraint;

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
            BackgroundColor = UIColor.Clear;
            TranslatesAutoresizingMaskIntoConstraints = false;

            InitializeRateBox();
            InitializeFixedBar();

            _fixedLowerBarView.TouchUpInside += (sender, e) => ToggleBaseRate();
		}

		public void ToggleBaseRate ()
		{
            if (!DisplayBaseRateInfo || BaseRate.BaseRate == null)
            {
                BaseRateToggled = false;
                return;
            }

			BaseRateToggled = !BaseRateToggled;
            _rateBoxHeightConstraint.Constant = BaseRateToggled ? BaseRateHeight : 0f;

            UIView.Animate(0.5f, 
                () => {
                    LayoutIfNeeded();
                    if(!BaseRateToggled)
                    {
						Layer.Mask = null;
                        OrderOptionsControl.Resize();
                    }
                },
                () => {
                    if(BaseRateToggled)
                    {
                        OrderOptionsControl.Resize();
                    }
                    LayoutSubviews();
                }
            );
		}

        private OrderOptionsControl OrderOptionsControl
        {
            get
            {
                return ((OrderOptionsControl)Superview.Superview);
            }
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
				_selectedVehicle = value;

                EtaBadge.Image = ImageHelper.ApplyColorToImage (string.Format ("{0}_no_badge_selected.png", value.LogoName.ToLower ()), Theme.LabelTextColor);
                BaseRate.BaseRate = value.BaseRate;
                if (value.BaseRate == null && BaseRateToggled)
                {
                    ToggleBaseRate();
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

        private void InitializeRateBox()
        {
            BaseRate = new BaseRateControl ();
            AddSubview(BaseRate);

            _rateBoxHeightConstraint = NSLayoutConstraint.Create(BaseRate, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f);

            AddConstraints (new [] {
                _rateBoxHeightConstraint,
                NSLayoutConstraint.Create (BaseRate, NSLayoutAttribute.Left, NSLayoutRelation.Equal, BaseRate.Superview, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create (BaseRate, NSLayoutAttribute.Right, NSLayoutRelation.Equal, BaseRate.Superview, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create (BaseRate, NSLayoutAttribute.Top, NSLayoutRelation.Equal, BaseRate.Superview, NSLayoutAttribute.Top, 1f, 0f)
            });
        }

        private void InitializeFixedBar()
        {
            _fixedLowerBarView = new UIControl 
            {
                UserInteractionEnabled = true,
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.CompanyColor
            };
            AddSubview(_fixedLowerBarView);

            AddConstraints(new [] { 
                NSLayoutConstraint.Create(_fixedLowerBarView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, BaseRate, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(_fixedLowerBarView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, EtaLabelHeight),
                NSLayoutConstraint.Create(_fixedLowerBarView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _fixedLowerBarView.Superview, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(_fixedLowerBarView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, _fixedLowerBarView.Superview, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(_fixedLowerBarView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, _fixedLowerBarView.Superview, NSLayoutAttribute.Right, 1f, 0f),
            });

            EtaBadge = new UIImageView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = false
            };
            _fixedLowerBarView.AddSubview(EtaBadge);

            AddConstraints (new [] {
                NSLayoutConstraint.Create (EtaBadge, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, DisplayBaseRateInfo ? ImageSize.Height : EtaLabelHeight),
                NSLayoutConstraint.Create (EtaBadge, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, DisplayBaseRateInfo ? ImageSize.Width : 0f),
                NSLayoutConstraint.Create (EtaBadge, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EtaBadge.Superview, NSLayoutAttribute.Left, 1f, 4f),
                NSLayoutConstraint.Create (EtaBadge, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, EtaBadge.Superview, NSLayoutAttribute.CenterY, 1f, 0f)
            });

            EtaLabel = new UILabel 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = false,
                AdjustsFontSizeToFitWidth = true,
                BackgroundColor = UIColor.Clear,
                Lines = 1,
                Font = UIFont.FromName (FontName.HelveticaNeueLight, 30 / 2),
                TextAlignment = UITextAlignment.Center,
                TextColor = Theme.LabelTextColor,
                ShadowColor = UIColor.Clear
            };
            _fixedLowerBarView.AddSubview(EtaLabel);

            AddConstraints (new [] {
                NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Height, 1f, 0f),
                NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EtaBadge, NSLayoutAttribute.Right, 1f, 4f),
                NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Right, 1f, -4f)
            });
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
