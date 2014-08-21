using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("FlatTextField")]
	public class FlatTextField : UITextField
	{
	    private const float RadiusCorner = 2;
        protected const float Padding = 6.5f;
        private UIImageView _leftImageView;
        private UIView _shadowView = null;

	    public FlatTextField (IntPtr handle) : base (handle)
		{
			Initialize();
		}

		public FlatTextField ()
		{
			Initialize();
		}

		public FlatTextField (RectangleF frame) : base (frame)
		{
			Initialize();
		}

        private void Initialize ()
		{
			this.ShouldChangeCharacters = CheckMaxLength;

			BackgroundColor = Enabled ? UIColor.White : UIColor.Clear;

			if (UIHelper.IsOS7orHigher) 
            {
				TintColor = UIColor.FromRGB (44, 44, 44); // cursor color
				TextAlignment = UITextAlignment.Natural;
			} 
            else 
            {
				if (this.Services ().Localize.IsRightToLeft)
				{
					TextAlignment = UITextAlignment.Right;
				}
			}

            TextColor = UIColor.FromRGB(44, 44, 44);
			Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);

            //padding
            LeftView = new UIView(new RectangleF(0f, 0f, Padding, 1f)); 
			LeftViewMode = UITextFieldViewMode.Always;
            RightView = new UIView(new RectangleF(Frame.Right - Padding, 0f, Padding, 1f));
            RightViewMode = UITextFieldViewMode.UnlessEditing;
            ClearButtonMode = UITextFieldViewMode.WhileEditing;

			HasRightArrow = Enabled && HasRightArrow;
		}

		public override void Draw (RectangleF rect)
		{   
            var fillColor = BackgroundColor;
			var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, RadiusCorner);

			HasRightArrow = Enabled && HasRightArrow;

			DrawBackground(UIGraphics.GetCurrentContext(), rect, roundedRectanglePath, fillColor.CGColor);
			DrawStroke(fillColor.CGColor);
			SetNeedsDisplay();
		}

		public override bool Enabled 
        {
            get { return base.Enabled; }
			set 
            {
				base.Enabled = value;
				BackgroundColor = value ? UIColor.White : UIColor.Clear;
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

                    var image = UIImage.FromFile(value);

                    if (_leftImageView != null)
                    {
                        _leftImageView.RemoveFromSuperview();
                    }

                    _leftImageView = new UIImageView(new RectangleF(0, (Frame.Height - image.Size.Height)/2, image.Size.Width, image.Size.Height));
                    _leftImageView.Image = image;
                    AddSubview(_leftImageView);

                    // Adjust the left padding of the text for image width
                    LeftView.Frame = LeftView.Frame.SetWidth(image.Size.Width + Padding);

                    // And the right padding
                    RightView = new UIView(new RectangleF(Frame.Right - Padding, 0f, Padding, 1f));

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
                        var image = UIImage.FromFile ("right_arrow.png");
                        _rightArrow = new UIImageView (new RectangleF (Frame.Width - image.Size.Width - Padding, (Frame.Height - image.Size.Height) / 2, image.Size.Width, image.Size.Height));
                        _rightArrow.Image = image;

                        RightView.Frame = RightView.Frame.IncrementWidth (image.Size.Width + Padding); // this is to keep the same padding between the end of the text and the right arrow
                        AddSubview (_rightArrow);

                        SetNeedsDisplay ();
                    }
                    else
                    {
                        if (_rightArrow != null)
                        {
                            var imageWidth = _rightArrow.Image != null ? _rightArrow.Image.Size.Width : 0;
                            RightView.Frame = RightView.Frame.IncrementWidth (-(imageWidth + Padding));
                            _rightArrow.RemoveFromSuperview ();

                            SetNeedsDisplay ();
                        }
                    }
                }
            }
        }

		private void DrawBackground(CGContext context, RectangleF rect, UIBezierPath roundedRectanglePath, CGColor fillColor)
		{
			context.SaveState ();
			context.BeginTransparencyLayer (null);
			roundedRectanglePath.AddClip ();
            context.SetFillColorWithColor(fillColor);
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
                    _shadowView.Layer.ShadowOffset = new SizeF(0.3f, 0.3f);
                    _shadowView.Layer.ShouldRasterize = true;             
                    this.Superview.InsertSubviewBelow(_shadowView, this);
                }
                _shadowView.Frame = Frame.Copy().Shrink(1);
            }
		}

        protected virtual void DrawText(CGContext context, RectangleF rect, CGColor textColor)
        {
            //Hook?
        }

		public int? MaxLength { get; set; }

		private bool CheckMaxLength (UITextField textField, NSRange range, string replacementString)
		{
			if (MaxLength.HasValue) {
				int textLength = Text.HasValue () ? Text.Length : 0;
				int replaceLength = replacementString.HasValue () ? replacementString.Length : 0;
				int newLength = textLength + replaceLength - range.Length;
				return (newLength <= MaxLength);
			} else {
				return true;
			}
		}

	}
}

