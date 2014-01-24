using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("FlatTextView")]
    public class FlatTextView : UITextView
    {
        NSMutableParagraphStyle _paragraphStyle;

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

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                var attributedText = new NSMutableAttributedString(value, font: UIFont.FromName(FontName.HelveticaNeueRegular, 14f), foregroundColor: UIColor.FromRGB(44, 44, 44), paragraphStyle: _paragraphStyle);
                base.AttributedText = attributedText;
            }
        }
    }
}

