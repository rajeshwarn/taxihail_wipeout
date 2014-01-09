using System;
using System.Drawing;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
    public sealed class SingleLinePictureCell : MvxTableViewCell
    {
        private UIImageView _picture;
        private UILabel _rightText;
        private UILabel _leftText;
        private UIImageView _plusSignImage;
        private UIButton _removeButton;
        private ICommand _deleteCommand;
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
        
        public string LeftText { get { return _leftText.Text ;}
            set { _leftText.Text = value; }
        }
        
        public string RightText { get { return _rightText.Text; }
            set { _rightText.Text = value; }
        }

        public string Picture{
            get{
                return _picture.Image.ToString();}
            set
            { 
                _picture.Image = UIImage.FromFile("Assets/CreditCard/"+value.ToLower()+".png");
            }
        }
        
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
                if(_isLast)
                {
                    _removeButton.Hidden =true;
                }
            }
        }
        public bool IsAddNewCell {
            set {
                ((CustomCellBackgroundView)BackgroundView).IsAddNewCell = value;
                _rightText.TextColor = _leftText.TextColor = value 
                    ? AppStyle.CellAddTextColor 
                    : AppStyle.CellFirstLineTextColor;
            }
        }

        public ICommand DeleteCommand{
            get{ return _deleteCommand;}
            set{
                _deleteCommand = value;
                    AddRemoveButton();
            }
        }
        
        private void Initialize ()
        {
            _removeButton = new UIButton (new RectangleF (245, _rowHeight/2 - 48/2, 76, 48 ) );
            BackgroundView = new CustomCellBackgroundView( IsFirst, IsLast, Frame, ShowPlusSign );
           
            _rightText = new UILabel(new RectangleF(150, _rowHeight/2 - 15/2,80,15));
            _rightText.TextColor = AppStyle.CellSecondLineTextColor;
            _rightText.BackgroundColor = UIColor.Clear;
            _rightText.Font = AppStyle.NormalTextFont;
            AddSubview(_rightText);

            _leftText = new UILabel(new RectangleF(60, _rowHeight/2 - 15/2,200,15));
            _leftText.TextColor = AppStyle.CellFirstLineTextColor;
            _leftText.BackgroundColor = UIColor.Clear;
            _leftText.Font = AppStyle.CellFont;
            AddSubview(_leftText);

            _picture = new UIImageView (new RectangleF (12, (_rowHeight - 32) / 2, 46, 32 ) ); 
            _picture.BackgroundColor = UIColor.Clear;
            AddSubview ( _picture ); 
            
            _plusSignImage = new UIImageView (new RectangleF (16, (_rowHeight - 24) / 2, 38, 24 ) ); 
            _plusSignImage.BackgroundColor = UIColor.Clear;
            _plusSignImage.Image = UIImage.FromFile("Assets/Cells/add_btn.png");
            _plusSignImage.Hidden = true;
            AddSubview ( _plusSignImage );  
           
        }

        public void AddRemoveButton(){
             
            _removeButton.BackgroundColor = UIColor.Clear;
            _removeButton.ContentMode = UIViewContentMode.ScaleAspectFit;
            _removeButton.SetImage(UIImage.FromFile("Assets/CreditCard/delete_card_btn.png"),UIControlState.Normal);
            _removeButton.TouchUpInside += (sender, e) => {
                DeleteCommand.Execute();
            };
            AddSubview ( _removeButton ); 
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