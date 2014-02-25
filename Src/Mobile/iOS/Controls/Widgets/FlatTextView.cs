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
        private NSMutableParagraphStyle _paragraphStyle;
        private static UIColor DefaultFontColor = UIColor.FromRGB(44, 44, 44);
        private static UIColor PlaceholderFontColor = UIColor.FromRGB(200, 200, 200);
        private UILabel _lblPlaceholder;

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

            _lblPlaceholder = new UILabel(new RectangleF(5, 5, 0, 0)) { TextColor = PlaceholderFontColor };
            AddSubview(_lblPlaceholder);
        }

        public override UIFont Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                _lblPlaceholder.Font = value;
            }
        }

        public void TapAnywhereToClose(Func<UIView> owner)
        {
            var giantInvisibleButton = new UIButton();

            ShouldBeginEditing = t => 
            {
                var o = owner();
                o.AddSubview(giantInvisibleButton);
                giantInvisibleButton.Frame = o.Frame.Copy();

                _lblPlaceholder.Hidden = true;

                return true;
            };

            ShouldEndEditing = t => 
            {           
                giantInvisibleButton.RemoveFromSuperview();

                if (string.IsNullOrWhiteSpace(Text))
                {
                    _lblPlaceholder.Hidden = false;
                }
                else
                {
                    _lblPlaceholder.Hidden = true;
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
                var attributedText = new NSMutableAttributedString(value, font: Font, foregroundColor: DefaultFontColor, paragraphStyle: _paragraphStyle);
                base.AttributedText = attributedText;
            }
        }
            
        public string Placeholder
        {
            get { return _lblPlaceholder.Text; }
            set
            {
                _lblPlaceholder.Text = value;
                _lblPlaceholder.SizeToFit();
            }
        }
    }
}

