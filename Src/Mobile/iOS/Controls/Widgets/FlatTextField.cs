using System;
using Foundation;
using UIKit;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Common.Extensions;
using CoreAnimation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("FlatTextField")]
	public class FlatTextField : UITextField
	{
	    private const float RadiusCorner = 2;
        protected nfloat LeftPadding = 6.5f;
        protected nfloat RightPadding = 6.5f;
        private UIView _shadowView = null;

        public bool MoveClearButtonFromUnderRightImage { get; set; }

	    public FlatTextField (IntPtr handle) : base (handle)
		{
			Initialize();
		}

		public FlatTextField (): base()
		{
			Initialize();
		}

		public FlatTextField (CGRect frame) : base (frame)
		{
			Initialize();
		}

        public void Initialize ()
		{
			this.ShouldChangeCharacters = CheckMaxLength;

			this.BackgroundColor = UIColor.White;

            TextAlignment = UITextAlignment.Natural;
            TintColor = UIColor.FromRGB (44, 44, 44); // cursor color
            TextColor = UIColor.FromRGB(44, 44, 44);
			Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);

            //padding
            LeftView = GetRegularLeftViewForPadding(); 
			LeftViewMode = UITextFieldViewMode.Always;
            RightView = new UIView();
            RightViewMode = UITextFieldViewMode.UnlessEditing;
            ClearButtonMode = UITextFieldViewMode.WhileEditing;
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

		private UIColor _backgroundColor;
		public override UIColor BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;
				base.BackgroundColor = value;
			}
		}

		public override bool Enabled 
        {
            get { return base.Enabled; }
			set 
            {
                if (base.Enabled != value)
                {
                    base.Enabled = value;
                    base.BackgroundColor = value ? BackgroundColor : UIColor.Clear;

                    SetNeedsDisplay();
                }
			}
		}

        public bool ShowShadow { get; set; }

        public bool DisableRoundCorners { get; set; }

        private string _imageLeftSource;
        public string ImageLeftSource
        {
            get { return _imageLeftSource; }
            set
            {
                _imageLeftSource = value;

                if (value.HasValue())
                {
                    var image = UIImage.FromBundle(value);

                    LeftView = new UIImageView(new CGRect(
                        0, 
                        (Frame.Height - image.Size.Height) / 2, 
                        image.Size.Width + LeftPadding, 
                        image.Size.Height)) { Image = image };
                }
                else
                {
                    if (LeftView != null)
                    {
                        LeftView.RemoveFromSuperview();
                        LeftView.Dispose();
                        LeftView = GetRegularLeftViewForPadding();
                    }
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

                    ShowOrHideRightArrow();
                }
            }
        }

	    private void ShowOrHideRightArrow()
	    {
			if (HasRightArrow)
	        {
	            if (_rightArrow == null)
	            {
	                _rightArrow = new UIImageView {Image = UIImage.FromFile("right_arrow.png")};
	                AddSubview(_rightArrow);
	            }
	        }
	        else
	        {
	            if (_rightArrow != null)
	            {
	                _rightArrow.Image = null;
	                _rightArrow.RemoveFromSuperview();
	                _rightArrow.Dispose();
	                _rightArrow = null;
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

			RightView.Frame = new CGRect(Frame.Right - RightPadding, 0f, RightPadding, Frame.Height);

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

        public override void Draw (CGRect rect)
        {   
            DrawStroke();
        }

        private void DrawStroke()
        {
            if (!DisableRoundCorners)
            {
                this.Layer.Mask = GetMaskForRoundedCorners();
            }

            DrawShadow();
        }

        private CAShapeLayer GetMaskForRoundedCorners()
        {
            var roundedRectanglePath = UIBezierPath.FromRoundedRect (Bounds, RadiusCorner);
            var biggerRect = Bounds.Copy().Grow(5);

            var maskPath = new UIBezierPath();
            maskPath.MoveTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMaxY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMaxY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMinY()));
            maskPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));
            maskPath.AppendPath(roundedRectanglePath);

            var maskForRoundedCorners = new CAShapeLayer();
            var newPath = new CGPath();
            newPath.AddRect(biggerRect);
            newPath.AddPath(maskPath.CGPath);
            maskForRoundedCorners.Path = newPath;
            maskForRoundedCorners.FillRule = CAShapeLayer.FillRuleEvenOdd;

            newPath.Dispose();
            maskPath.Dispose();
            roundedRectanglePath.Dispose();

            return maskForRoundedCorners;
        }

        private CGPath GetShadowPath(CGRect biggerRect)
        {
            var shadowPath = new UIBezierPath();
            shadowPath.MoveTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));
            shadowPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMaxY()));
            shadowPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMaxY()));
            shadowPath.AddLineTo(new CGPoint(biggerRect.GetMaxX(), biggerRect.GetMinY()));
            shadowPath.AddLineTo(new CGPoint(biggerRect.GetMinX(), biggerRect.GetMinY()));;
            shadowPath.AppendPath(UIBezierPath.FromRoundedRect (Bounds, RadiusCorner));
            shadowPath.UsesEvenOddFillRule = true;

            return shadowPath.CGPath;
        }

        private void DrawShadow()
        {
            ClearShadowIfNecessary();

            if (ShowShadow)
            {
                var biggerRect = Bounds.Copy().Grow(2);

                _shadowView = new UIView(Frame);
                _shadowView.Layer.MasksToBounds = false;
                _shadowView.Layer.ShadowColor = UIColor.Black.CGColor;
                _shadowView.Layer.ShadowOpacity = 0.5f;
                _shadowView.Layer.ShadowOffset = new CGSize(0f, 0f);
                _shadowView.Layer.ShadowPath = GetShadowPath(biggerRect);
                _shadowView.Layer.ShouldRasterize = true;   
                _shadowView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
                this.Superview.InsertSubviewBelow(_shadowView, this);   
            }
        }

        private void ClearShadowIfNecessary()
        {
            if (_shadowView != null)
            {
                _shadowView.RemoveFromSuperview();
                _shadowView.Dispose();
                _shadowView = null;
            }
        }

        public nint? MaxLength { get; set; }
		private bool CheckMaxLength (UITextField textField, NSRange range, string replacementString)
		{
			if (MaxLength.HasValue) 
            {
				var textLength = Text.HasValue () ? Text.Length : 0;
                var replaceLength = replacementString.HasValue () ? replacementString.Length : 0;
                var newLength = textLength + replaceLength - range.Length;
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

        private UIView GetRegularLeftViewForPadding()
        {
            return new UIView(new CGRect(0f, 0f, LeftPadding, this.Frame.Height));
        }
	}
}

