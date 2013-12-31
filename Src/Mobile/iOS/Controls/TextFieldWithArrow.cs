using System;
using apcurium.MK.Booking.Mobile.Client.Helper;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TextFieldWithArrow: UIView
    {
        TextField _textField;
        UIButton _button;
        UIImageView _leftImage;
  
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
            BackgroundColor = UIColor.Clear;

            _textField = new TextField(Bounds);
            _textField.UserInteractionEnabled = false;
            _textField.VerticalAlignment = UIControlContentVerticalAlignment.Center;

            _leftImage = new UIImageView(new Rectangle(5, 0, 0, 0));

            var rightArrow = new UIImageView(new RectangleF(Frame.Width - 25, Frame.Height/2 - 7,9, 13));
            rightArrow.Image = UIImage.FromFile("Assets/Cells/rightArrow.png");
            
            _button = new UIButton(Bounds);
            AddSubview(_textField);
            AddSubview(rightArrow);
            AddSubview(_button);
        }

        public string Text {
            set {
                _textField.Text = value;
            }
        }

        protected UIButton Button
        { 
            get {
                return _button;
            }
        }

        public UITextField TextField
        {
            get{
                return _textField;
            }
        }

        public string LeftImagePath
        {
            set{
                var image = ImageHelper.GetImage(value);
                _leftImage = new UIImageView(new RectangleF(5, 0, image.Size.Width, image.Size.Height));
                _leftImage.Image = image;
                TextField.LeftViewMode = UITextFieldViewMode.Always;
                TextField.LeftView =_leftImage;
            }
        }
    }
}

