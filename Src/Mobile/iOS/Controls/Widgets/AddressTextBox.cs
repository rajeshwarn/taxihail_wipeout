using System;
using UIKit;
using Foundation;
using System.Linq;
using apcurium.MK.Common.Extensions;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AddressTextBox")]
    public class AddressTextBox : UIView
    {        
        public event EventHandler AddressClicked;

        public Action<string> AddressUpdated;

        private FlatTextField StreetNumberTextView { get; set; }
        private FlatTextField AddressTextView { get; set; }
        public UIButton AddressButton { get; set; }
        private UIActivityIndicatorView LoadingWheel  { get; set; }
        private UIView VerticalDivider { get; set; }
        private UIView HorizontalDividerTop { get; set; }
        private UIView StreetNumberRoundedCornerView { get; set; }
        private NSLayoutConstraint _streetNumberTextViewWidthConstraint;
        private const float MinimumStreetNumberTextViewWidth = 40f;

        public AddressTextBox(IntPtr h):base(h)
        {
            Initialize();
        }
        public AddressTextBox ( )
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            StreetNumberRoundedCornerView = new UIView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Hidden = true
            };
            AddSubview(StreetNumberRoundedCornerView);

            StreetNumberTextView = new FlatTextField 
            {
                DisableRoundCorners = true,
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Clear,
                ClearButtonMode = UITextFieldViewMode.Never,
                Placeholder = "#",
                AccessibilityLabel = Localize.GetValue("StreetNumber"),
                KeyboardType = UIKeyboardType.NumberPad,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                TextAlignment = UITextAlignment.Center
            };
            StreetNumberTextView.ShowCloseButtonOnKeyboard();
            AddSubview(StreetNumberTextView);

            _streetNumberTextViewWidthConstraint = 
                NSLayoutConstraint.Create(StreetNumberTextView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, MinimumStreetNumberTextViewWidth);
            AddConstraints(new [] {
                NSLayoutConstraint.Create(StreetNumberTextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, StreetNumberTextView.Superview, NSLayoutAttribute.Left, 1f, 0f),
                _streetNumberTextViewWidthConstraint,
                NSLayoutConstraint.Create(StreetNumberTextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, StreetNumberTextView.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(StreetNumberTextView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, StreetNumberTextView.Superview, NSLayoutAttribute.Height, 1f, 0f)
            });

            AddConstraints(new [] {
                NSLayoutConstraint.Create(StreetNumberRoundedCornerView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(StreetNumberRoundedCornerView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(StreetNumberRoundedCornerView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(StreetNumberRoundedCornerView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Height, 1f, 0f)
            });

            AddressTextView = new FlatTextField
            {
                DisableRoundCorners = true,
                BackgroundColor = UIColor.Clear,
                ClipsToBounds = true,
                VerticalAlignment = UIControlContentVerticalAlignment.Center
            };   
            AddSubview(AddressTextView);

            AddressButton = new UIButton();
            AddressButton.TouchDown += (sender, e) => {
				if(!UserInputDisabled && AddressClicked != null)
                {
                    AddressClicked(this, EventArgs.Empty);
                }
            };
            AddSubview(AddressButton);

            LoadingWheel = new UIActivityIndicatorView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Color = UIColor.Gray
            };
            AddSubview(LoadingWheel);
            AddConstraints(new [] {
                NSLayoutConstraint.Create(LoadingWheel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Left, 1f, 0f), 
                NSLayoutConstraint.Create(LoadingWheel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(LoadingWheel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(LoadingWheel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Height, 1f, 0f)
            });
            
            VerticalDivider = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.FromRGB(118, 118, 118)
            };
            AddSubview(VerticalDivider);
            AddConstraints(new [] {
                NSLayoutConstraint.Create(VerticalDivider, NSLayoutAttribute.Left, NSLayoutRelation.Equal, StreetNumberTextView, NSLayoutAttribute.Right, 1f, 0f), 
                NSLayoutConstraint.Create(VerticalDivider, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, UIHelper.OnePixel),
                NSLayoutConstraint.Create(VerticalDivider, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, VerticalDivider.Superview, NSLayoutAttribute.CenterY, 1f, 0f),
                NSLayoutConstraint.Create(VerticalDivider, NSLayoutAttribute.Height, NSLayoutRelation.Equal, VerticalDivider.Superview, NSLayoutAttribute.Height, 0.73f, 0f)
            });

            HorizontalDividerTop = new UIView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.FromRGB(177, 177, 177)
            };
            AddSubview(HorizontalDividerTop);
            AddConstraints(new [] {
                NSLayoutConstraint.Create(HorizontalDividerTop, NSLayoutAttribute.Left, NSLayoutRelation.Equal, HorizontalDividerTop.Superview, NSLayoutAttribute.Left, 1f, 0f), 
                NSLayoutConstraint.Create(HorizontalDividerTop, NSLayoutAttribute.Width, NSLayoutRelation.Equal, HorizontalDividerTop.Superview, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(HorizontalDividerTop, NSLayoutAttribute.Top, NSLayoutRelation.Equal, HorizontalDividerTop.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(HorizontalDividerTop, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, UIHelper.OnePixel)
            });

            SetBehavior();

            Resize();
        }

		public bool UserInputDisabled { get; set; }

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
                    new [] { VerticalDivider, HorizontalDividerTop }.Where(c => c != null).ForEach(c => c.Hidden = value);   
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

        bool _isLoadingAddress;
        public bool IsLoadingAddress
        {
            get { return _isLoadingAddress; }
            set 
            {
                _isLoadingAddress = value;
                if (value && IsSelected)
                {
                    ShowLoadingWheel();
                }
                else
                {
                    HideLoadingWheel();
                }
            }
        }

        private void ShowLoadingWheel()
        {
            LoadingWheel.StartAnimating();
            LoadingWheel.Hidden = false;
            StreetNumberTextView.Hidden = true;
        }

        private void HideLoadingWheel()
        {
            LoadingWheel.StopAnimating();
            LoadingWheel.Hidden = true;
            StreetNumberTextView.Hidden = !IsSelected;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }        
            set
            {
                _isSelected = value;
                StreetNumberTextView.Hidden = !value;
                Resize();
            }
        }

        private bool _isDestination;
        public bool IsDestination
        {
            get { return _isDestination; }        
            set
            {
                _isDestination = value;
                Resize();
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (IsSelected)
            {
                StreetNumberTextView.Hidden = false;

                AddressButton.Frame = AddressTextView.Frame = new CGRect(VerticalDivider.Frame.Right + 6, 0, this.Frame.Width - VerticalDivider.Frame.Right, this.Frame.Height);

                AddressTextView.LeftViewMode = UITextFieldViewMode.Never;
            }
            else
            {
                StreetNumberTextView.Hidden = true;

                AddressButton.Frame = AddressTextView.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height);

                AddressTextView.SetPadding(VerticalDivider.Frame.Right + 6, 6);
                AddressTextView.LeftView = new Dot(6, GetColor(), -3)
                { 
                    Frame = new CGRect(0, 0, VerticalDivider.Frame.Right + 6, this.Frame.Height)
                };
                AddressTextView.LeftViewMode = UITextFieldViewMode.Always;
            }
        }

        private Address _currentAddress;
        public Address CurrentAddress
        {
            get { return _currentAddress; }
            set
            {
                _currentAddress = value;
                if (AddressTextView != null && _currentAddress != null)
                {
                    AddressTextView.Text = _currentAddress.DisplayAddress;
                }
            }
        }

        private void Resize()
        {
            AddressTextView.UserInteractionEnabled = IsSelected;

            var sizeThatFits = StreetNumberTextView.GetSizeThatFits(StreetNumberTextView.Text, StreetNumberTextView.Font);
            _streetNumberTextViewWidthConstraint.Constant = (float)Math.Max(sizeThatFits.Width + 30f, MinimumStreetNumberTextViewWidth);

            if (IsSelected)
            {
                StreetNumberRoundedCornerView.BackgroundColor = GetColor();
                HorizontalDividerTop.Hidden = !IsDestination;
            }

            SetNeedsLayout();
        }

        private void SetBehavior()
        {
            //Order is important
            NumberAndAddressTextFieldBehavior.ApplyTo(AddressTextView, StreetNumberTextView, () => CurrentAddress, number => 
            {
                if (AddressUpdated != null)
                {
                    AddressUpdated(number);
                }
            });

            StreetNumberTextView.TapAnywhereToClose(() => this.Superview.Superview.Superview);

            StreetNumberTextView.ValueChanged += (sender, e) => Resize();

            StreetNumberTextView.EditingDidBegin += (sender, e) => 
            {
                if(string.IsNullOrWhiteSpace(StreetNumberTextView.Text))
                {
                    StreetNumberTextView.ResignFirstResponder();
                    if(AddressClicked != null)
                    {
                        AddressClicked(this, EventArgs.Empty);
                    }
                    return;
                }

                StreetNumberRoundedCornerView.Hidden = false;
                VerticalDivider.Hidden = true;
                Resize();
            };

            StreetNumberTextView.EditingDidEnd += (sender, e) => 
            {
                StreetNumberRoundedCornerView.Hidden = true;
                VerticalDivider.Hidden = false;
                Resize();
            };
        }

        private UIColor GetColor()
        {
            return IsDestination ? UIColor.FromRGB(255, 0, 0) : UIColor.FromRGB(30, 192, 34);
        }
    }
}