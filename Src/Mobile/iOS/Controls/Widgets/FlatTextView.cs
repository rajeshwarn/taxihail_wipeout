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
        UIColor DefaultFontColor = UIColor.FromRGB(44, 44, 44);
        UIColor PlaceholderFontColor = UIColor.FromRGB(200, 200, 200);

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

            Font = UIFont.FromName(FontName.HelveticaNeueRegular, 14f);
            FontColor = DefaultFontColor;
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
                    FontColor = DefaultFontColor;
                    Text = string.Empty;
                }
                return true;
            };
            ShouldEndEditing = t => 
            {           
                giantInvisibleButton.RemoveFromSuperview();

                if (string.IsNullOrWhiteSpace(Text))
                {
                    FontColor = PlaceholderFontColor;
                    Text = Placeholder;
                }
                return true;
            };

            giantInvisibleButton.TouchDown += (sender, e) => 
            {
                EndEditing(true);
            };
        }

        private UIFont _font;
        public override UIFont Font
        {
            get { return _font; }
            set
            {
                _font = value;
                Text = Text;
            }
        }

        private UIColor _fontColor;
        public UIColor FontColor
        {
            get { return _fontColor; }
            set
            {
                _fontColor = value;
                Text = Text;
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                var attributedText = new NSMutableAttributedString(value, font: Font, foregroundColor: FontColor, paragraphStyle: _paragraphStyle);
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
                    FontColor = PlaceholderFontColor;
                    Text = value;
                }
            }
        }
    }
}

