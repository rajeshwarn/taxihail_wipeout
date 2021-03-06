using System;
using UIKit;
using Foundation;
using CoreGraphics;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("FlatTextView")]
    public class FlatTextView : UITextView
    {
        private NSMutableParagraphStyle _paragraphStyle;
        private static UIColor DefaultFontColor = UIColor.FromRGB(44, 44, 44);
        private static UIColor DefaultPlaceholderFontColor = UIColor.FromRGB(200, 200, 200);
	    private UILabel _lblPlaceholder;

        public FlatTextView (IntPtr handle) : base (handle)
        {
            Initialize();
        }

        public FlatTextView (CGRect frame) : base (frame)
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

            _lblPlaceholder = new UILabel 
            {
				TextColor = DefaultPlaceholderFontColor, 
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
                SetNeedsLayout();
            }
        }

	    public UIColor PlaceholderColor
	    {
			get { return _lblPlaceholder.TextColor; }
		    set
		    {
			    _lblPlaceholder.TextColor = value;
				SetNeedsLayout();
		    }
	    }

	    public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _lblPlaceholder.Frame = new CGRect(5, 5, Frame.Width, Frame.Height);
            _lblPlaceholder.SizeToFit();
        }
    }
}

