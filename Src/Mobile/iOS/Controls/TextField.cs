using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Style;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Register("TextField")]
    public class TextField : UITextField
    {

        private bool _isProgressing = false;
        private UIColor _strokeColor = UIColor.FromRGBA(82, 82, 82, 255);
        private float _paddingRight = 0;
        private float _paddingLeft = 0;
        private float _paddingBaseLeft = 5;
        private UIActivityIndicatorView _progress;



        public TextField(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        public TextField(RectangleF rect) : base( rect )
        {
            Initialize();
        }

       
        public void Initialize()
        {
            BorderStyle = UITextBorderStyle.Line;
            BackgroundColor = UIColor.Clear;
            TextColor = UIColor.FromRGB(64, 64, 64);
            Font = AppStyle.NormalTextFont;
            PaddingLeft = _paddingBaseLeft;
			LeftViewMode = UITextFieldViewMode.Always;
            this.Bounds = new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, 40);
        }

        public float FieldHeight
        {
            set
            {
                this.Bounds = new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, value);
            }
        }

        public float PaddingLeft
        {
            get{ return _paddingLeft; }
            set
            {
                _paddingLeft = value;

				if (LeftView != null)
					LeftView.Frame = new RectangleF(LeftView.Frame.X, LeftView.Frame.Y, _paddingLeft, LeftView.Frame.Height);
				else
					LeftView = new UIView{ Frame = new RectangleF(Frame.X, Frame.Y,_paddingLeft,30) };
            }
        }

        public float PaddingRight
        {
            get{ return _paddingRight; }
            set
            {
                _paddingRight = value;
            }
                
        }
         
        public bool IsProgressing
        {
            get { return _isProgressing; }
            set
            {
                _isProgressing = value;
                RefreshUI();
            }
        }

        private void InitProgress()
        {
			var r  = new RectangleF(5, Frame.Height / 2 - ImageLeft.Image.Size.Height / 2, ImageLeft.Image.Size.Width, ImageLeft.Image.Size.Height);
            _progress = new UIActivityIndicatorView(r );
            _progress.BackgroundColor = UIColor.Clear;
            _progress.Hidden = true;
            _progress.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray;
            AddSubview( _progress );
        }
        private void RefreshUI()
        { 
			ImageLeft.Hidden = _isProgressing ;

            if ( _progress == null )
            {
                InitProgress();
            }

            _progress.Hidden = !_isProgressing;
            if (_isProgressing)
            {
                _progress.StartAnimating();
            }
            else
            {
                _progress.StopAnimating();
            }   
        }

		protected UIImageView ImageLeft { get; set; }

		private string _imageLeftSource;
		public string ImageLeftSource
        {
			get {return _imageLeftSource;}
            set
            {
				_imageLeftSource = value;
				SetImageLeft(_imageLeftSource);
            }
        }

		protected UIImageView ImageRight { get; set; }

		private string _imageRightSource;
		public string ImageRightSource
		{
			get {return _imageRightSource;}
			set
			{
				_imageRightSource = value;
				SetImageRight(_imageRightSource);
			}
		}


		public void SetImageLeft(string image)
        {
			if (image != null)
			{
				var img = UIImage.FromFile(image);

				ImageLeft = new UIImageView(new RectangleF(0, 0, img.Size.Width, img.Size.Height));

				ImageLeft.BackgroundColor = UIColor.Clear;
				ImageLeft.Image = img;

				LeftView = new UIView{ Frame = new RectangleF(Frame.X, Frame.Y,ImageLeft.Frame.Width, ImageLeft.Frame.Height) };

				LeftView.AddSubview(ImageLeft);

				PaddingLeft = _paddingBaseLeft + img.Size.Width;
			}
			else
				PaddingLeft = _paddingBaseLeft;
        }


		public void SetImageRight(string image)
		{

		}


        public UIColor StrokeColor
        {
            get { return _strokeColor;}
            set { _strokeColor = value; }
        }

        public override void Draw(System.Drawing.RectangleF frame)
        {

            // General Declarations
            var context = UIGraphics.GetCurrentContext();

            // Color Declarations
            UIColor color = UIColor.FromRGBA(0.00f, 0.00f, 0.00f, 1.00f);
            UIColor color3 = color.ColorWithAlpha(0.6f);
            UIColor wColor = UIColor.FromRGBA(1.00f, 1.00f, 1.00f, 1.00f);
            UIColor color2 = wColor.ColorWithAlpha(0.2f);


            // Shadow Declarations
            var inner = color3.CGColor;
            var innerOffset = new SizeF(0, 1);

            var radius = StyleManager.Current.TextboxCornerRadius.HasValue ? StyleManager.Current.TextboxCornerRadius.Value : 3;
            var innerBlurRadius = radius- 1;
            var outer = color2.CGColor;
            var outerOffset = new SizeF(0, 1);
            var outerBlurRadius = innerBlurRadius-1;

            // Rectangle Drawing
            context.SetShadowWithColor(outerOffset, outerBlurRadius, outer);


            var rectanglePath = UIBezierPath.FromRoundedRect(new RectangleF(frame.GetMinX() + 1f, frame.GetMinY() + 1f, frame.Width - 2, frame.Height - 2), radius);
            UIColor.White.SetFill();
            rectanglePath.Fill();

            // Rectangle Inner Shadow
            var rectangleBorderRect = rectanglePath.Bounds;
            rectangleBorderRect.Inflate(innerBlurRadius, innerBlurRadius);
            rectangleBorderRect.Offset(-innerOffset.Width, -innerOffset.Height);
            rectangleBorderRect = RectangleF.Union(rectangleBorderRect, rectanglePath.Bounds);
            rectangleBorderRect.Inflate(1, 1);

            var rectangleNegativePath = UIBezierPath.FromRect(rectangleBorderRect);
            rectangleNegativePath.AppendPath(rectanglePath);
            rectangleNegativePath.UsesEvenOddFillRule = true;

            context.SaveState();
            {
                var xOffset = innerOffset.Width + (float)Math.Round(rectangleBorderRect.Width);
                var yOffset = innerOffset.Height;
                context.SetShadowWithColor(new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)), innerBlurRadius, inner);

                rectanglePath.AddClip();
                var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(rectangleBorderRect.Width), 0);
                rectangleNegativePath.ApplyTransform(transform);
                UIColor.Gray.SetFill();
                rectangleNegativePath.Fill();
            }
            context.RestoreState();


            context.SaveState();
            rectanglePath.LineWidth = 1f;
            _strokeColor.SetStroke();
            rectanglePath.AddClip();
            context.AddPath(rectanglePath.CGPath);
            context.StrokePath();
            context.RestoreState();           

        }
    }
}