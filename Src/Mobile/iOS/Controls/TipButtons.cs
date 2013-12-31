using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("TipButtons")]
    public class TipButtons: UIView
    {
        public event EventHandler<TipButtonsValueChangedEventArgs> ValueChanged;

        GradientButton _tipAmountButton;
        GradientButton _tipPercentButton;

        public TipButtons (RectangleF rect): base(rect)
        {
            Initialize();
        }

        public TipButtons(IntPtr handle):base(handle)
        {
            Initialize();
        }

        void Initialize ()
        {
            BackgroundColor = UIColor.Clear;

            _tipPercentButton = new SegmentedGradientButton(new RectangleF(Bounds.X, Bounds.Y, Bounds.Width / 2, Bounds.Height))
            {
                IsLeftButton = true
            };
            AppButtons.FormatStandardButton(_tipPercentButton, "%", AppStyle.ButtonColor.SegmentedLightBlue);
            _tipPercentButton.RoundedCorners = UIRectCorner.BottomLeft | UIRectCorner.TopLeft;
            _tipPercentButton.TouchUpInside += HandleTouchUpInside;

            _tipAmountButton = new SegmentedGradientButton(new RectangleF(Bounds.X + Bounds.Width / 2, Bounds.Y, Bounds.Width / 2, Bounds.Height))
            {
                IsLeftButton = false
            };
            AppButtons.FormatStandardButton(_tipAmountButton, string.Empty, AppStyle.ButtonColor.SegmentedLightBlue);
            _tipAmountButton.RoundedCorners = UIRectCorner.BottomRight | UIRectCorner.TopRight;
            _tipAmountButton.TouchUpInside += HandleTouchUpInside;

            AddSubviews(_tipPercentButton, _tipAmountButton);

            IsTipInPercent = true;

        }

        string tipCurrency;
        public string TipCurrency {
            get {
                return tipCurrency;
            }
            set {
                tipCurrency = value;
                _tipAmountButton.SetTitle(value, UIControlState.Normal);
            }
        } 

        void HandleTouchUpInside (object sender, EventArgs e)
        {
            var buttonIndex = (_tipPercentButton == sender) ? 0 : 1;
            if (ValueChanged != null) {
                ValueChanged(this, new TipButtonsValueChangedEventArgs(buttonIndex));
            }
        }

        private bool _isTipInPercent;
        public bool IsTipInPercent {
            get {
                return _isTipInPercent;
            }
            set {
                _isTipInPercent = value;
                _tipPercentButton.Selected = value;
                _tipAmountButton.Selected = !value;
            }
        }
    }

    public class TipButtonsValueChangedEventArgs: EventArgs
    {
        public TipButtonsValueChangedEventArgs (int buttonIndex)
        {
            ButtonIndex = buttonIndex;
        }

        public int ButtonIndex {
// ReSharper disable once UnusedAutoPropertyAccessor.Global
            get;
            private set;
        }
    }
     
}

