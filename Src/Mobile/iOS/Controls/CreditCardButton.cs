using System;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("CreditCardButton")]
    public class CreditCardButton: NavigateTextField
    {
        public CreditCardButton(IntPtr handle) : base(  handle )
        {
            Initialize();
        }
        
        
        public CreditCardButton(RectangleF rect) 
            : base ( rect )
        {
            Initialize();
        }

        void Initialize()
        {
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
                    var image = UIImage.FromFile("Assets/CreditCard/" +value.ToLower() +".png");
                    if(image != null)
                    {
                        Button.SetImage(image, UIControlState.Normal);
                        Button.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
                        TextField.LeftViewMode = UITextFieldViewMode.Always;
                        TextField.LeftView = new UIView(new Rectangle(0, 0, (int)image.Size.Width, (int)image.Size.Height));
                    }
                    else
                    {
                        Button.SetImage(null, UIControlState.Normal);
                        TextField.LeftViewMode = UITextFieldViewMode.Never;
                    }
                }
            }
        }
    }
}

