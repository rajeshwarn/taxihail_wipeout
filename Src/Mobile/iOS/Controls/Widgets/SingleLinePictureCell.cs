using System;
using System.Drawing;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helper;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public sealed class SingleLinePictureCell : MvxStandardTableViewCell
    {
        private bool _hideBottomBar;
        private UILabel _leftText;
        private UILabel _rightText;
        private string _icon;
        private bool _isAddNewCell;
        private ICommand _deleteCommand;
        private SizeF StandardImageSize = new SizeF(46f, 32f);

        public SingleLinePictureCell (IntPtr handle, string bindingText) : base(bindingText, handle)
        {       
        }
        
        public SingleLinePictureCell (string cellIdentifier, string bindingText) : base(bindingText, UITableViewCellStyle.Subtitle, new NSString(cellIdentifier), UITableViewCellAccessory.None)
        {                   
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;
            Initialize ();
        }

        public bool HideBottomBar
        {
            get { return _hideBottomBar; }
            set
            { 
                if (BackgroundView is CustomCellBackgroundView)
                {
                    ((CustomCellBackgroundView)BackgroundView).HideBottomBar = value;
                }
                _hideBottomBar = value;
            }
        }

        public string LeftText 
        { 
            get { return _leftText.Text; }
            set { _leftText.Text = value; }
        }

        public string RightText 
        { 
            get { return _rightText.Text; }
            set { _rightText.Text = value; }
        }

        public string Icon 
        { 
            get { return _icon; }
            set 
            { 
                _icon = value;
                if (IsAddNewCell)
                {
                    ImageView.Image = UIImage.FromFile("add_list.png").ResizeCanvas(StandardImageSize);
                }
                else
                {
                    ImageView.Image = value.HasValue() 
                                      ? UIImage.FromFile(string.Format("{0}.png", value)) 
                                      : UIImage.FromFile("credit_card_generic.png");
                }
            }
        }

        public bool IsAddNewCell 
        {
            get { return _isAddNewCell; }
            set 
            {
                _isAddNewCell = value;

                if (value)
                {
                    _leftText.TextColor = _rightText.TextColor = UIColor.FromRGB(44, 44, 44);
                    _leftText.Font = UIFont.FromName(FontName.HelveticaNeueMedium, 32/2);
                }
            }
        }

        public ICommand DeleteCommand
        {
            get { return _deleteCommand; }
            set
            {
                _deleteCommand = value;
                AddRemoveButton();
            }
        }

        private void Initialize ()
        {
			BackgroundView = new CustomCellBackgroundView(Frame, 10, UIColor.White, UIColor.FromRGB(190, 190, 190));
           
            _leftText = new UILabel(new RectangleF(65, (Frame.Height - 15)/2, 200, 15));
            _leftText.TextColor = UIColor.FromRGB(44, 44, 44);
            _leftText.Font = UIFont.FromName(FontName.HelveticaNeueLight, 32/2);
            _leftText.BackgroundColor = UIColor.Clear;
            AddSubview(_leftText);

            _rightText = new UILabel(new RectangleF(190, (Frame.Height - 15)/2, 80, 15));
            _rightText.TextColor = UIColor.FromRGB(44, 44, 44);
            _rightText.Font = UIFont.FromName(FontName.HelveticaNeueMedium, 32/2);
            _rightText.BackgroundColor = UIColor.Clear;
            AddSubview(_rightText);
        }

        public void AddRemoveButton()
        {
            if (!IsAddNewCell)
            {
                var removeButton = new UIButton(new RectangleF(this.Frame.Width - 38 - 10, (this.Frame.Height - 24) / 2, 38, 24));
                removeButton.BackgroundColor = UIColor.Clear;
                removeButton.ContentMode = UIViewContentMode.ScaleAspectFit;
                removeButton.SetImage(UIImage.FromFile("delete_card_btn.png"),UIControlState.Normal);
                removeButton.TouchUpInside += (sender, e) => {
                    DeleteCommand.Execute();
                };
                AddSubview (removeButton); 
            }
        }
        
        public override void TouchesBegan (NSSet touches, UIEvent evt)
        {   
            ((CustomCellBackgroundView)BackgroundView).Highlighted = true;
            SetNeedsDisplay();
            base.TouchesBegan ( touches, evt ); 
        }

        public override void TouchesEnded (NSSet touches, UIEvent evt)
        {
            ((CustomCellBackgroundView) BackgroundView).Highlighted = false;
            SetNeedsDisplay();
            base.TouchesEnded (touches, evt);
        }

        public override void TouchesCancelled (NSSet touches, UIEvent evt)
        {
            ((CustomCellBackgroundView) BackgroundView).Highlighted = false;
            SetNeedsDisplay();
            base.TouchesCancelled (touches, evt);
        }
    }
}