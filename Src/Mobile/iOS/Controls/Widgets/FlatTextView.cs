using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Common.Extensions;

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

            _lblPlaceholder = new UILabel(new RectangleF(5, 5, Frame.Width, Frame.Height)) 
            { 
                TextColor = PlaceholderFontColor, 
                Lines = 0, 
                LineBreakMode = UILineBreakMode.WordWrap, 
                TextAlignment = UITextAlignment.Left,
                BackgroundColor = UIColor.Clear
            };
            AddSubview(_lblPlaceholder);

            ShouldBeginEditing = t => 
            {
                _lblPlaceholder.Hidden = true;
                return true;
            };

            ShouldEndEditing = t => 
            {           
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

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _lblPlaceholder.Hidden = false;
                }
                else
                {
                    _lblPlaceholder.Hidden = true;
                }

                var attributedText = new NSMutableAttributedString(value.ToSafeString(), font: Font, foregroundColor: DefaultFontColor, paragraphStyle: _paragraphStyle);
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

