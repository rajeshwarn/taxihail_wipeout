using System;
using Foundation;
using UIKit;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("FlatTextField")]
	public class FlatTextField : UITextField
	{
	    private const float RadiusCorner = 2;
        protected nfloat LeftPadding = 6.5f;
        protected nfloat RightPadding = 6.5f;
        private UIImageView _leftImageView;
        private UIView _shadowView = null;

        public bool MoveClearButtonFromUnderRightImage { get; set; }

	    public FlatTextField (IntPtr handle) : base (handle)
		{
			Initialize();
		}

		public FlatTextField ()
		{
			Initialize();
		}

		public FlatTextField (CGRect frame) : base (frame)
		{
			Initialize();
		}

        private void Initialize ()
		{
			this.ShouldChangeCharacters = CheckMaxLength;

            BackgroundColor = Enabled 
                ? UIColor.White 
                : UIColor.Clear;

            HasRightArrow = Enabled && HasRightArrow;

            TextAlignment = NaturalLanguageHelper.GetTextAlignment();

			if (UIHelper.IsOS7orHigher) 
            {
				TintColor = UIColor.FromRGB (44, 44, 44); // cursor color
			} 

            TextColor = UIColor.FromRGB(44, 44, 44);
			Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);

            //padding
            LeftView = new UIView(); 
			LeftViewMode = UITextFieldViewMode.Always;
            RightView = new UIView();
            RightViewMode = UITextFieldViewMode.UnlessEditing;
            ClearButtonMode = UITextFieldViewMode.WhileEditing;
		}

		public override void Draw (CGRect rect)
		{   
            var fillColor = BackgroundColor;
			var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, RadiusCorner);
            if (!ForceWhiteBackground)
            {
                HasRightArrow = Enabled && HasRightArrow;
            }
			DrawBackground(UIGraphics.GetCurrentContext(), rect, roundedRectanglePath, fillColor.CGColor);
			DrawStroke(fillColor.CGColor);
			SetNeedsDisplay();
		}

        bool _forceWhiteBackground;
        public bool ForceWhiteBackground
        {
            get
            {
                return _forceWhiteBackground;
            }
            set
            {
                _forceWhiteBackground = value;
                BackgroundColor = UIColor.White;
                SetNeedsDisplay();
            }
        }

		public override bool Enabled 
        {
            get { return base.Enabled; }
			set 
            {
				base.Enabled = value;
                BackgroundColor = value ? BackgroundColor : UIColor.Clear;
				SetNeedsDisplay();
			}
		}

        public bool ShowShadow { get; set; }

        private string _imageLeftSource;
        public string ImageLeftSource
        {
            get { return _imageLeftSource; }
            set
            {
                if (value.HasValue() && value != _imageLeftSource)
                {
                    _imageLeftSource = value;

                    var image = UIImage.FromBundle(value);

                    // remove previous image if it exists
                    if (_leftImageView != null)
                    {
                        _leftImageView.RemoveFromSuperview();
                    }

                    _leftImageView = new UIImageView 
                    {
                        Image = image
                    };
                    AddSubview(_leftImageView);

                    SetNeedsDisplay();
                }
            }
        }
            
        private UIImageView _rightArrow { get; set; }
        private bool _hasRightArrow { get; set; }
        public bool HasRightArrow 
        {
            get { return _hasRightArrow; }
            set 
            {
                if (_hasRightArrow != value)
                {
                    _hasRightArrow = value;

                    if (value)
                    {
                        _rightArrow = new UIImageView
                        {
                            Image = UIImage.FromFile ("right_arrow.png")
                        };
                        AddSubview (_rightArrow);

                        SetNeedsDisplay ();
                    }
                    else
                    {
                        if (_rightArrow != null)
                        {
                            _rightArrow.RemoveFromSuperview ();

                            SetNeedsDisplay ();
                        }
                    }
                }
            }
        }

        public void SetPadding(nfloat left, nfloat right)
        {
            LeftPadding = left;
            RightPadding = right;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            LeftView.Frame = new CGRect(0f, 0f, LeftPadding, this.Frame.Height);
            RightView.Frame = new CGRect(Frame.Right - RightPadding, 0f, RightPadding, this.Frame.Height);

            if (ImageLeftSource.HasValue())
            {
                if (_leftImageView != null && _leftImageView.Image != null)
                {
                    _leftImageView.Frame = new CGRect(
                        0, 
                        (Frame.Height - _leftImageView.Image.Size.Height) / 2, 
                        _leftImageView.Image.Size.Width, 
                        _leftImageView.Image.Size.Height);

                    // Adjust the left padding of the text for image width
                    LeftView.Frame = LeftView.Frame.SetWidth(_leftImageView.Image.Size.Width + LeftPadding);
                }
            }

            if (HasRightArrow)
            {
                if (_rightArrow != null)
                {
                    _rightArrow.Frame = new CGRect(
                        Frame.Width - _rightArrow.Image.Size.Width - RightPadding, 
                        (Frame.Height - _rightArrow.Image.Size.Height) / 2, 
                        _rightArrow.Image.Size.Width, 
                        _rightArrow.Image.Size.Height);

                    // this is to keep the same padding between the end of the text and the right arrow
                    RightView.Frame = RightView.Frame.IncrementWidth(_rightArrow.Image.Size.Width + RightPadding); 
                }
            }
            else
            {
                if (_rightArrow != null)
                {
                    var imageWidth = _rightArrow.Image != null ? _rightArrow.Image.Size.Width : 0;
                    RightView.Frame = RightView.Frame.IncrementWidth (-(imageWidth + RightPadding));
                    _rightArrow = null;
                }
            }
        }

		private void DrawBackground(CGContext context, CGRect rect, UIBezierPath roundedRectanglePath, CGColor fillColor)
		{
			context.SaveState ();
			context.BeginTransparencyLayer (null);
			roundedRectanglePath.AddClip ();
            context.SetFillColor(fillColor);
			context.FillRect(rect);
			context.EndTransparencyLayer ();
			context.RestoreState ();
		}

		private void DrawStroke(CGColor fillColor)
		{
			BorderStyle = UITextBorderStyle.None;
			Layer.BorderWidth = 1.0f;
			Layer.BorderColor = fillColor;
			Layer.CornerRadius = RadiusCorner;

            if (ShowShadow)
            {
                if (_shadowView == null)
                {
                    _shadowView = new UIView(Frame);
                    _shadowView.BackgroundColor = UIColor.White.ColorWithAlpha(0.7f);
                    _shadowView.Layer.MasksToBounds = false;
                    _shadowView.Layer.ShadowColor = UIColor.FromRGBA(0, 0, 0, 127).CGColor;
                    _shadowView.Layer.ShadowOpacity = 1.0f;
                    _shadowView.Layer.ShadowRadius = RadiusCorner + 1;
                    _shadowView.Layer.ShadowOffset = new CGSize(0.3f, 0.3f);
                    _shadowView.Layer.ShouldRasterize = true;             
                    this.Superview.InsertSubviewBelow(_shadowView, this);
                }
                _shadowView.Frame = Frame.Copy().Shrink(1);
            }
		}

        protected virtual void DrawText(CGContext context, CGRect rect, CGColor textColor)
        {
            //Hook?
        }

        public nint? MaxLength { get; set; }
		private bool CheckMaxLength (UITextField textField, NSRange range, string replacementString)
		{
			if (MaxLength.HasValue) 
            {
				nint textLength = Text.HasValue () ? Text.Length : 0;
                nint replaceLength = replacementString.HasValue () ? replacementString.Length : 0;
                nint newLength = textLength + replaceLength - range.Length;
				return newLength <= MaxLength;
			} 
            else 
            {
				return true;
			}
		}

        public override CGRect ClearButtonRect(CGRect forBounds)
        {
            var rect = base.ClearButtonRect(forBounds);

            if (MoveClearButtonFromUnderRightImage)
            {
                rect.X -= 25;
            }

            return rect;
        }
	}
}

