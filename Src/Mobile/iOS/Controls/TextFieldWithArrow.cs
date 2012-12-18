using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TextFieldWithArrow: UIView
    {
        UIImageView _rightArrow;
        TextField _textField;
        UIButton _button;
  
        public TextFieldWithArrow(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        public TextFieldWithArrow(RectangleF rect) : base( rect )
        {
            Initialize();
        }

        void Initialize ()
        {
            this.BackgroundColor = UIColor.Clear;

            _textField = new TextField(this.Bounds);
            _textField.UserInteractionEnabled = false;
            _textField.VerticalAlignment = UIControlContentVerticalAlignment.Center;
            
            var rightArrow = new UIImageView(new RectangleF(this.Frame.Width - 25, this.Frame.Height/2 - 7,9, 13));
            rightArrow.Image = UIImage.FromFile("Assets/Cells/rightArrow.png");
            
            this._button = new UIButton(this.Bounds);
            this.AddSubview(_textField);
            this.AddSubview(rightArrow);
            this.AddSubview(_button);
        }

        public string Text {
            set {
                this._textField.Text = value;
            }
        }

        protected UIButton Button
        { 
            get {
                return _button;
            }
        }
    }
}

