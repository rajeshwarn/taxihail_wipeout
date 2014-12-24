using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Linq;
using apcurium.MK.Common.Extensions;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AddressTextBox")]
    public class AddressTextBox : UIView
    {        
        private bool _isInStreetNumberEditMode;

        public event EventHandler AddressClicked;

        public Action<string> AddressUpdated;

        private FlatTextField StreetNumberTextView { get; set; }
        public FlatTextField AddressTextView { get; set; }
        private UIButton AddressButton { get; set; }
        private UIActivityIndicatorView LoadingWheel  { get; set; }
        private UIView VerticalDivider { get; set; }
        private UIView HorizontalDividerTop { get; set; }
        private RoundedCornerView StreetNumberRoundedCornerView { get; set; }

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

            StreetNumberRoundedCornerView = new RoundedCornerView();
            StreetNumberRoundedCornerView.Hidden = true;
            AddSubview(StreetNumberRoundedCornerView);

            StreetNumberTextView = new FlatTextField();
            StreetNumberTextView.BackgroundColor = UIColor.Clear;
            StreetNumberTextView.Placeholder = "#";
            StreetNumberTextView.SetLeftPadding(15);
            StreetNumberTextView.SetRightPadding(0);
            StreetNumberTextView.KeyboardType = UIKeyboardType.NumberPad;
            StreetNumberTextView.ShowCloseButtonOnKeyboard();
            StreetNumberTextView.VerticalAlignment = UIControlContentVerticalAlignment.Center;
            AddSubview(StreetNumberTextView);

            AddressTextView = new FlatTextField();   
            AddressTextView.BackgroundColor = UIColor.Clear;
            AddressTextView.ClipsToBounds = true;
            AddressTextView.VerticalAlignment = UIControlContentVerticalAlignment.Center;
            AddSubview(AddressTextView);

            AddressButton = new UIButton();
            AddressButton.TouchDown += (sender, e) => {
				if(!UserInputDisabled && AddressClicked != null)
                {
                    AddressClicked(this, EventArgs.Empty);
                }
            };
            AddSubview(AddressButton);

            LoadingWheel = new UIActivityIndicatorView();
            LoadingWheel.Color = UIColor.Gray;
            AddSubview(LoadingWheel);

            VerticalDivider = new UIView();
            VerticalDivider.BackgroundColor = UIColor.FromRGB(118, 118, 118);
            AddSubview(VerticalDivider);

            HorizontalDividerTop = new UIView();
            HorizontalDividerTop.BackgroundColor = UIColor.FromRGB(177, 177, 177);
            AddSubview(HorizontalDividerTop);

            SetBehavior();

            Resize();
        }

		public bool UserInputDisabled { get; set; }

        public string Address
        {
            get { return AddressTextView.Text; }
            set { AddressTextView.Text = value; }
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

        private void Resize()
        {
            AddressTextView.UserInteractionEnabled = IsSelected;

            var color = IsDestination ? UIColor.FromRGB(255, 0, 0) : UIColor.FromRGB(30, 192, 34);

            if (IsSelected)
            {
                StreetNumberTextView.Hidden = false;
                StreetNumberTextView.SizeToFit();
                StreetNumberTextView.SetHeight(this.Frame.Height).IncrementWidth(10);

                VerticalDivider.Frame = new RectangleF(StreetNumberTextView.Frame.Right, 6, UIHelper.OnePixel, this.Frame.Height - 12);
                HorizontalDividerTop.Frame = new RectangleF(0, 0, this.Frame.Width, UIHelper.OnePixel);
                AddressButton.Frame = AddressTextView.Frame = new RectangleF(VerticalDivider.Frame.Right + 6, 0, this.Frame.Width - VerticalDivider.Frame.Right, this.Frame.Height);

                AddressTextView.LeftViewMode = UITextFieldViewMode.Never;

                StreetNumberRoundedCornerView.BackColor = color;
                StreetNumberRoundedCornerView.StrokeLineColor = color;

                if (IsDestination)
                {
                    StreetNumberRoundedCornerView.Corners = 0;
                    HorizontalDividerTop.Hidden = false;
                }
                else
                {
                    StreetNumberRoundedCornerView.Corners = UIRectCorner.TopLeft | UIRectCorner.BottomLeft;
                    HorizontalDividerTop.Hidden = true;
                }

                StreetNumberRoundedCornerView.Frame = LoadingWheel.Frame = StreetNumberTextView.Frame;

                if (_isInStreetNumberEditMode)
                {
                    StreetNumberTextView.IncrementWidth(50);
                }
            }
            else
            {
                StreetNumberTextView.Hidden = true;
                AddressButton.Frame = AddressTextView.Frame = new RectangleF(0, 0, this.Frame.Width, this.Frame.Height);
                AddressTextView.LeftView = new Dot(6, color, -3)
                {
                    Frame = new RectangleF(0, 0, VerticalDivider.Frame.Right + 6, this.Frame.Height)
                };
                AddressTextView.LeftViewMode = UITextFieldViewMode.Always;
            }
        }

        private void SetBehavior()
        {
            //Order is important
            NumberAndAddressTextFieldBehavior.ApplyTo(AddressTextView, StreetNumberTextView, number => 
                {
                    if (AddressUpdated != null)
                    {
                        AddressUpdated(number);
                    }
                });

            StreetNumberTextView.TapAnywhereToClose(()=>this.Superview.Superview.Superview);

            StreetNumberTextView.EditingChanged += (sender, e) => 
            {
                Resize();
            };

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
                _isInStreetNumberEditMode = true;
                Resize();
            };

            StreetNumberTextView.EditingDidEnd += (sender, e) => 
            {
                StreetNumberRoundedCornerView.Hidden = true;
                VerticalDivider.Hidden = false;
                _isInStreetNumberEditMode = false;
                Resize();
            };
        }
    }
}