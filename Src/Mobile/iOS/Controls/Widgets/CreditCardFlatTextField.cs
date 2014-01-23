using System;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("FlatCreditCardTextField")]
    public class FlatCreditCardTextField : NavigateFlatTextField
    {
        public FlatCreditCardTextField (IntPtr handle) : base (handle)
        {
        }

        public FlatCreditCardTextField () : base()
        {
        }

        public FlatCreditCardTextField (RectangleF frame) : base (frame)
        {
        }

        [UsedImplicitly]
        private string _last4Digits { get; set; }
        public string Last4Digits {
            set {
                _last4Digits = string.IsNullOrEmpty (value) ? string.Empty : "\u2022\u2022\u2022\u2022 " + value;
                RightView = new UILabel(new RectangleF(0, 0, 100, Bounds.Height))
                {
                    Text = _last4Digits,
                    BackgroundColor = UIColor.Clear,
                    TextColor = UIColor.FromRGB(133, 133, 133),
                    Font = UIFont.FromName(FontName.HelveticaNeueMedium, 32/2)
                };

                RightViewMode = UITextFieldViewMode.Always;
            }
            get {
                return _last4Digits;
            }
        }

        [UsedImplicitly]
        public string CreditCardCompany{
            set
            { 
                if(value != null)
                {
                    var image = UIImage.FromFile(value.ToLower() + ".png");
                    if(image != null)
                    {
                        Button.SetImage(image, UIControlState.Normal);
                        Button.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
                        LeftViewMode = UITextFieldViewMode.Always;
                        LeftView = new UIView(new Rectangle(0, 0, (int)image.Size.Width, (int)image.Size.Height));
                    }
                    else
                    {
                        Button.SetImage(null, UIControlState.Normal);
                        LeftViewMode = UITextFieldViewMode.Never;
                    }
                }
                else {
                    Button.SetImage(null, UIControlState.Normal);
                    LeftViewMode = UITextFieldViewMode.Never;
                }
            }
        }
    }
}

