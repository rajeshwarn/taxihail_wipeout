using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using System.Globalization;
using MonoTouch.CoreGraphics;
using System.Linq;

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
            var configManager = TinyIoC.TinyIoCContainer.Current.Resolve<IConfigurationManager>();
            var culture = CultureInfo.GetCultureInfo( configManager.GetSetting("PriceFormat"));

            this.BackgroundColor = UIColor.Clear;

            var btnStyle = StyleManager.Current.Buttons.Single( b => b.Key == AppStyle.ButtonColor.LightBlue.ToString() );

            _tipPercentButton = new SegmentedGradientButton(new RectangleF(this.Bounds.X, this.Bounds.Y, this.Bounds.Width / 2, this.Bounds.Height))
            {
                IsLeftButton = true
            };
            AppButtons.FormatStandardButton(_tipPercentButton, "%", AppStyle.ButtonColor.LightBlue);
            _tipPercentButton.TitleColour = UIColor.FromRGB(112,112,112).CGColor;
            _tipPercentButton.SelectedTitleColour = UIColor.White.CGColor;
            _tipPercentButton.TextShadowColor = null;
            _tipPercentButton.SelectedTextShadowColor = UIColor.FromRGBA(0,0,0,128);
            _tipPercentButton.RoundedCorners = UIRectCorner.BottomLeft | UIRectCorner.TopLeft;
            _tipPercentButton.TouchUpInside += HandleTouchUpInside;

            _tipAmountButton = new SegmentedGradientButton(new RectangleF(this.Bounds.X + this.Bounds.Width / 2, this.Bounds.Y, this.Bounds.Width / 2, this.Bounds.Height))
            {
                IsLeftButton = false
            };
            AppButtons.FormatStandardButton(_tipAmountButton, culture.NumberFormat.CurrencySymbol, AppStyle.ButtonColor.LightBlue);
            _tipAmountButton.TitleColour = UIColor.FromRGB(112,112,112).CGColor;
            _tipAmountButton.SelectedTitleColour = UIColor.White.CGColor;
            _tipAmountButton.TextShadowColor = null;
            _tipAmountButton.SelectedTextShadowColor = UIColor.FromRGBA(0,0,0,128);
            _tipAmountButton.RoundedCorners = UIRectCorner.BottomRight | UIRectCorner.TopRight;
            _tipAmountButton.TouchUpInside += HandleTouchUpInside;
            
            this.AddSubviews(_tipPercentButton, _tipAmountButton);

            IsTipInPercent = true;

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
            this.ButtonIndex = buttonIndex;
        }

        public int ButtonIndex {
            get;
            private set;
        }
    }
     
}

