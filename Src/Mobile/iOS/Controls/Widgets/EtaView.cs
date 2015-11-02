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
    	private const float EtaLabelHeight = 44f;
		private const float BaseRateHeight = 100f;

        private CGSize ImageSize = new CGSize(34f, 34f);

        private UIControl _fixedLowerBarView { get; set; }
		private BaseRateControl BaseRate { get; set; }
        private UIImageView EtaBadge { get; set; }
        private UILabel EtaLabel { get; set; }
        private UIImageView ArrowImage { get; set; }

        private NSLayoutConstraint _constraintEtaBadgeHeight;
        private NSLayoutConstraint _constraintEtaBadgeWidth;
        private NSLayoutConstraint _constraintArrowImageWidth;

        private bool _baseRateToggled;
        private bool BaseRateToggled
        {
            get
            {
                return _baseRateToggled;
            }
            set
            {
                _baseRateToggled = value;
                // TODO this will only work for dark company colors
                ArrowImage.Highlighted = value;
            }
        }

        private bool _displayBaseRateInfo;
        public bool DisplayBaseRateInfo
        {
            get
            {
                return _displayBaseRateInfo;
            }
            set
            {
                _displayBaseRateInfo = value;

                if (_constraintEtaBadgeHeight != null)
                {
                    _constraintEtaBadgeHeight.Constant = !_displayBaseRateInfo ? ImageSize.Height : EtaLabelHeight;
                    _constraintEtaBadgeWidth.Constant = !_displayBaseRateInfo ? ImageSize.Height : 0f;
                    _constraintArrowImageWidth.Constant = _displayBaseRateInfo ? 13f : 0f;
                }
            }
        }

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
				ArrowImage.Hidden = UserInputDisabled;
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

            UIView.AnimateNotify(0.5f, 
                () => {
                    LayoutIfNeeded();
                    if(!BaseRateToggled)
                    {
                        OrderOptionsControl.Resize(true);
                    }
                    OrderOptionsControl.Layer.Mask = null;
                },
                finished => {
                    if(finished)
                    {
                        OrderOptionsControl.Resize();
                        LayoutSubviews();
                    }
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

        private VehicleType _selectedVehicle;
        public VehicleType SelectedVehicle
		{
			get { return _selectedVehicle; }
			set
			{
                if (value != null)
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

            _constraintEtaBadgeHeight = NSLayoutConstraint.Create(EtaBadge, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, !DisplayBaseRateInfo ? ImageSize.Height : EtaLabelHeight);
            _constraintEtaBadgeWidth = NSLayoutConstraint.Create(EtaBadge, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, !DisplayBaseRateInfo ? ImageSize.Width : 0f);
            AddConstraints (new [] {
                _constraintEtaBadgeHeight,
                _constraintEtaBadgeWidth,
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

            ArrowImage = new UIImageView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = false
            };
            ArrowImage.Image = UIImage.FromBundle("down_arrow_light");
            ArrowImage.HighlightedImage = UIImage.FromBundle("up_arrow_light");
            _fixedLowerBarView.AddSubview(ArrowImage);

            AddConstraints (new [] {
                NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.Height, 1f, 0f),

				NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, EtaLabel.Superview, NSLayoutAttribute.CenterY, 1f, -4f),
                NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EtaBadge, NSLayoutAttribute.Right, 1f, 4f),
				NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ArrowImage, NSLayoutAttribute.Left, 1f, 0f)
            });

            _constraintArrowImageWidth = NSLayoutConstraint.Create(ArrowImage, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, DisplayBaseRateInfo ? 13f : 0f);
            AddConstraints (new [] {
                NSLayoutConstraint.Create (ArrowImage, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 8f),
                _constraintArrowImageWidth,
                NSLayoutConstraint.Create (ArrowImage, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ArrowImage.Superview, NSLayoutAttribute.Right, 1f, -4f),
                NSLayoutConstraint.Create (ArrowImage, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, ArrowImage.Superview, NSLayoutAttribute.CenterY, 1f, 0f)
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
