using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("FlatTextView")]
    public class FlatTextView : UITextView
    {
        NSMutableParagraphStyle _paragraphStyle;
        static UIColor DefaultFontColor = UIColor.FromRGB(44, 44, 44);
        static UIColor PlaceholderFontColor = UIColor.FromRGB(200, 200, 200);

        private static bool _showPlaceholder = true;

        public FlatTextView (IntPtr handle) : base (handle)
        {
            Initialize();
        }

        public FlatTextView (RectangleF frame) : base (frame)
        {
            Initialize();
        }

        public FlatTextView ()
        {
            Initialize();
        }

        private void Initialize()
        {
            _paragraphStyle = new NSMutableParagraphStyle();
            _paragraphStyle.LineHeightMultiple = 20f;
            _paragraphStyle.MinimumLineHeight = 20f;
            _paragraphStyle.MaximumLineHeight = 20f;
        }

        public void TapAnywhereToClose(Func<UIView> owner)
        {
            var giantInvisibleButton = new UIButton();

            ShouldBeginEditing = t => 
            {
                var o = owner();
                o.AddSubview(giantInvisibleButton);
                giantInvisibleButton.Frame = o.Frame.Copy();

                if (Text == Placeholder)
                {
                    _showPlaceholder = false;
                    Text = string.Empty;
                }
                return true;
            };
            ShouldEndEditing = t => 
            {           
                giantInvisibleButton.RemoveFromSuperview();

                _showPlaceholder = true;
                if (string.IsNullOrWhiteSpace(Text))
                {
                    Text = Placeholder;
                }
                return true;
            };

            giantInvisibleButton.TouchDown += (sender, e) => 
            {
                EndEditing(true);
            };
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) && _showPlaceholder)
                {
                    value = Placeholder;
                }

                if (value == Placeholder)
                {
                    TextColor = PlaceholderFontColor;
                }
                else
                {
                    TextColor = DefaultFontColor;
                }

                var attributedText = new NSMutableAttributedString(value, font: Font, foregroundColor: TextColor, paragraphStyle: _paragraphStyle);
                base.AttributedText = attributedText;
            }
        }

        private string _placeholder = string.Empty;
        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value;
                if (string.IsNullOrWhiteSpace(Text))
                {
                    _showPlaceholder = true;
                    Text = value;
                }
            }
        }
    }
}

