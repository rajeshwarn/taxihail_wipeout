using System;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Drawing;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
    public class SingleLinePictureCell : MvxBindableTableViewCell
    {
        private UIImageView _picture;
        private UILabel _rightText;
        private UILabel _leftText;
        private UIImageView _plusSignImage;
        private bool _showPlusSign;
        private bool _showArrow;
        private float _rowHeight = 44f;
        public SingleLinePictureCell (IntPtr handle, string bindingText) : base(bindingText, handle)
        {       
        }
        
        public SingleLinePictureCell (string cellIdentifier, string bindingText) : base( bindingText, UITableViewCellStyle.Subtitle, new NSString(cellIdentifier), UITableViewCellAccessory.None   )
        {                   
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;
            Initialize ();
        }
        
        public string LeftText { get { return this._leftText.Text ;}
            set { this._leftText.Text = value; }
        }
        
        public string RightText { get { return this._rightText.Text; }
            set { this._rightText.Text = value; }
        }

        public string Picture{
            get{
                return this._picture.Image.ToString();}
            set
            { 
                this._picture.Image = UIImage.FromFile("Assets/CreditCard/"+value.ToLower()+".png");
            }
        }
        
        public string test{get;set;}
        
        public bool ShowPlusSign { 
            get { return _showPlusSign; }
            set { 
                _showPlusSign = value;
                _plusSignImage.Hidden = !_showPlusSign;
            }
        }
        
        public bool ShowRightArrow { 
            get { return _showArrow; }
            set { 
                _showArrow = value;
                _picture.Hidden = !_showArrow;
            }
        }
        
        private bool _isFirst, _isLast;
        public bool IsFirst { get { return _isFirst;} 
            set{ 
                _isFirst = value;
                ((CustomCellBackgroundView)BackgroundView).IsTop = _isFirst;
            }
        }
        public bool IsLast { get{ return _isLast;} 
            set{ 
                _isLast = value;
                ((CustomCellBackgroundView)BackgroundView).IsBottom = _isLast;
            }
        }
        public bool IsAddNewCell {
            set {
                ((CustomCellBackgroundView)BackgroundView).IsAddNewCell = value;
            }
        }
        
        private void Initialize ()
        {
            BackgroundView = new CustomCellBackgroundView( IsFirst, IsLast, Frame, ShowPlusSign );
           
            _rightText = new UILabel(new RectangleF(250, _rowHeight/2 - 15/2,80,15));
            _rightText.TextColor = AppStyle.CellSecondLineTextColor;
            _rightText.BackgroundColor = UIColor.Clear;
            _rightText.Font = AppStyle.NormalTextFont;
            AddSubview(_rightText);

            _leftText = new UILabel(new RectangleF(70, _rowHeight/2 - 15/2,200,15));
            _leftText.TextColor = AppStyle.CellFirstLineTextColor;
            _leftText.BackgroundColor = UIColor.Clear;
            _leftText.Font = AppStyle.CellFont;
            AddSubview(_leftText);

            _picture = new UIImageView (new RectangleF (15, _rowHeight/2 - 15/2, 40, 15 ) ); 
            _picture.BackgroundColor = UIColor.Clear;
            _picture.ContentMode = UIViewContentMode.ScaleAspectFit;
            AddSubview ( _picture ); 
            
            _plusSignImage = new UIImageView (new RectangleF (260, _rowHeight/2 - 15/2, 14, 15 ) ); 
            _plusSignImage.BackgroundColor = UIColor.Clear;
            _plusSignImage.ContentMode = UIViewContentMode.ScaleAspectFit;
            _plusSignImage.Image = UIImage.FromFile("Assets/Cells/plusSign.png");
            _plusSignImage.Hidden = true;
            AddSubview ( _plusSignImage );  
        }
        
        public override void TouchesBegan ( NSSet touches, UIEvent evt )
        {   
            BackgroundView.SetNeedsDisplay();
            base.TouchesBegan ( touches, evt ); 
        }
        public override void TouchesEnded (NSSet touches, UIEvent evt)
        {
            SetHighlighted( false, true );
            BackgroundView.SetNeedsDisplay();
            base.TouchesEnded (touches, evt);
        }
        public override void TouchesCancelled (NSSet touches, UIEvent evt)
        {
            SetHighlighted( false, true );
            BackgroundView.SetNeedsDisplay();
            base.TouchesCancelled (touches, evt);
        }
        
        
    }
}