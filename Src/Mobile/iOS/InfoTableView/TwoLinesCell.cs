using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
    public sealed class TwoLinesCell : MvxBindableTableViewCell
    {
        private UIImageView _arrowImage;
        //private UIImageView _plusSignImage;
        private bool _showPlusSign;
        private bool _showArrow;
        private string _icon;

        public TwoLinesCell (string cellIdentifier, string bindingText) : base( bindingText, UITableViewCellStyle.Subtitle, new NSString(cellIdentifier), UITableViewCellAccessory.None   )
        {                   
            SelectionStyle = UITableViewCellSelectionStyle.None;
            
            Initialize ();
        }
        
        public string FirstLine { get { return TextLabel.Text; }
            set { TextLabel.Text = value; }
        }
        
		public UIColor FirstLineTextColor { get { return TextLabel.TextColor; } set { TextLabel.TextColor = value; } }

        public string SecondLine { get { return DetailTextLabel.Text; }
            set { DetailTextLabel.Text = value;}
        }
        
        public bool ShowPlusSign { 
            get { return _showPlusSign; }
            set { 
                _showPlusSign = value;
                ImageView.Image = value ? UIImage.FromFile("Assets/Cells/add_btn.png") : null;
            }
        }
        
        public string Icon { 
            get { return _icon; }
            set { 
                _icon = value;
                if ( ShowPlusSign )
                {
                    return;
                }
                ImageView.Image = value.HasValue () ? UIImage.FromFile(string.Format ("Assets/{0}.png", value)) : null;
            }
        }
        
        
        public bool ShowRightArrow { 
            get { return _showArrow; }
            set { 
                _showArrow = value;
                _arrowImage.Hidden = !_showArrow;
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
                TextLabel.TextColor = DetailTextLabel.TextColor = value 
                    ? AppStyle.CellAddTextColor 
                        : AppStyle.CellFirstLineTextColor;
            }
        }
        
        private void Initialize ()
        {
            BackgroundView = new CustomCellBackgroundView( IsFirst, IsLast, Frame, ShowPlusSign );
            TextLabel.TextColor = AppStyle.CellFirstLineTextColor;
            TextLabel.BackgroundColor = UIColor.Clear;
            TextLabel.Font = AppStyle.CellFont;
            
            DetailTextLabel.TextColor = AppStyle.CellSecondLineTextColor;
            DetailTextLabel.BackgroundColor = UIColor.Clear;
            DetailTextLabel.Font = AppStyle.NormalTextFont;
    
            _arrowImage = new UIImageView (new RectangleF (0,0, 14, 15 )); 
            _arrowImage.BackgroundColor = UIColor.Clear;
            _arrowImage.ContentMode = UIViewContentMode.ScaleAspectFit;
            _arrowImage.Image = UIImage.FromFile("Assets/Cells/rightArrow.png");
            _arrowImage.Hidden = true;
            
            AccessoryView = _arrowImage;
         
            
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
