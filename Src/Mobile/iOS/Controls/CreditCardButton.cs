using System;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("CreditCardButton")]
    public class CreditCardButton: GradientButton
    {
        public CreditCardButton(IntPtr handle) : base(  handle )
        {
            Initialize();
        }
        
        
        public CreditCardButton(RectangleF rect, float cornerRadius, Style.ButtonStyle buttonStyle, string title, UIFont titleFont, string image = null) 
            : base ( rect, cornerRadius, buttonStyle, title, titleFont, image )
        {
            Initialize();
        }

        void Initialize()
        {
            this.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            this.SetRightImage("Assets/Cells/rightArrow.png");
            this.ContentEdgeInsets = new UIEdgeInsets(0, 3, 0, 10);
        }

        private string _last4Digits = string.Empty;
        public string Last4Digits {
            set {
                _last4Digits = value ?? string.Empty;
                SetNeedsDisplay();
            }
        }

        public string CreditCardCompany{
            set
            { 
                if(value != null)
                {
                    this.SetImage("Assets/CreditCard/" +value.ToLower() +".png");
                }
            }
        }

        protected override void DrawText (RectangleF rect, MonoTouch.CoreGraphics.CGContext context, RectangleF imageRect, RectangleF rightImageRect)
        {
            base.DrawText (rect, context, imageRect, rightImageRect);

            context.SaveState ();

            context.SelectFont (TitleFont.Name, TitleFont.PointSize, CGTextEncoding.MacRoman);
            context.SetTextDrawingMode (CGTextDrawingMode.Fill);
            context.SetStrokeColor (TitleColour);
            if (TextShadowColor != null) {
                context.SetShadowWithColor (new SizeF (0f, -0.5f), 0.5f, TextShadowColor.CGColor);
            }
            context.SetFillColor (TitleColour);
            context.TextMatrix = new CGAffineTransform (1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f);
            var titleSize = ((NSString)_last4Digits).StringSize (TitleFont);

            context.ShowTextAtPoint (rect.Width - ContentEdgeInsets.Right - rightImageRect.Width - 10 - titleSize.Width, rect.GetMidY () + (TitleFont.PointSize / 3), _last4Digits);

            context.RestoreState ();

        }
    }
}

