using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Style;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("TextView")]
    public class TextView : UITextView 
    {
        private readonly UIColor _strokeColor = UIColor.FromRGBA(82, 82, 82, 255);
        private UITextView _placeholder;
        public TextView ()
        {
        }
        public TextView(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        public TextView(RectangleF rect) : base( rect )
        {
            Initialize();
        }
        
        private void Initialize()
        {

            BackgroundColor = UIColor.Clear;
            TextColor = UIColor.FromRGB(64, 64, 64);
            Font = AppStyle.NormalTextFont;
            Bounds = new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height );

            _placeholder = CreatePlaceholder();
            AddSubview(_placeholder);

            Changed += HandleTextChanged;
        }

        #region Placeholder
        private UITextView CreatePlaceholder()
        {
            var placeholder = new UITextView(new RectangleF(0,0,Bounds.Width, Bounds.Height) );
            placeholder.BackgroundColor = UIColor.Clear;
            placeholder.Layer.BorderColor = UIColor.Clear.CGColor;
            placeholder.Layer.BorderWidth = 0f;
            placeholder.UserInteractionEnabled = false;
            placeholder.Hidden = false;
            placeholder.Font = AppStyle.NormalTextFont;
            placeholder.TextColor = AppStyle.GreyText.ColorWithAlpha(0.5f);

            return placeholder;
        }

        private void HandleTextChanged (object sender, EventArgs e)
        {
            SetPlaceholder(Text.Length == 0);
        }

        public string Placeholder { 
            get { return _placeholder.Text; } 
            set { _placeholder.Text = value;}
        }

        private void SetPlaceholder( bool visible )
        {
            if( (visible && _placeholder.Hidden) || (!visible && !_placeholder.Hidden) )
            {
                _placeholder.Hidden = !visible;
            }
        }
        #endregion

        public override void Draw(RectangleF frame)
        {
            
            // General Declarations
            var context = UIGraphics.GetCurrentContext();
            
            // Color Declarations
            var color = UIColor.FromRGBA(0.00f, 0.00f, 0.00f, 1.00f);
            var color3 = color.ColorWithAlpha(0.6f);
            var wColor = UIColor.FromRGBA(1.00f, 1.00f, 1.00f, 1.00f);
            var color2 = wColor.ColorWithAlpha(0.2f);
            
            
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

