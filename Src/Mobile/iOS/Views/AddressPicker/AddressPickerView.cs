using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using TinyIoC;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views.AddressPicker
{
    [Register("AddressPickerView")]
    public class AddressPickerView : BaseBindableChildView<AddressPickerViewModel>
    {
        const string CellBindingText = @"
                {
                   'FirstLine':{'Path':'Line1'},                   
                   'SecondLine':{'Path':'Line2'},                   
                   'Icon':{'Path':'Icon'},            
                    'HideBottomBar':{'Path':'IsLast'},            
                }";


        const float Margin = 6;
        const float Margin2x = Margin*2;

        public AddressPickerView(IntPtr h) : base(h)
        {
            Initialize();            
        }


        FlatTextField AddressEditText { get; set; }
        UIButton CancelButton { get; set;}
        public GroupedAddressTableViewSource TableViewSource { get; set; }
        UITableView TableView {get; set;}

        void Initialize()
        {
            BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("background.png"));
            Hidden = true;

            if (UIHelper.IsOS7orHigher)
            {
                AddSubview(new UIView { Frame = new RectangleF( 0,-40,1000,80), BackgroundColor = BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("background.png")) }); //Patch for iOS 7
            }

            AddressEditText = new FlatTextField();
            AddressEditText.VerticalAlignment = UIControlContentVerticalAlignment.Center;
            AddressEditText.ClearButtonMode = UITextFieldViewMode.Always;

            CancelButton = new FlatButton();
            CancelButton.SetTitle(Localize.GetValue("CancelButton"), UIControlState.Normal);

            TableView = new UITableView(  new RectangleF(0,0,320,1000), UITableViewStyle.Grouped);
            TableView.SectionHeaderHeight = 10;
            TableView.BackgroundView = new  UIView { BackgroundColor = UIColor.Clear };
            TableView.BackgroundColor = UIColor.Clear;
            TableView.RowHeight = 45;
            TableView.AddGestureRecognizer( GetHideKeyboardOnTouchGesture()  );

            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            TableViewSource = new GroupedAddressTableViewSource(TableView, UITableViewCellStyle.Subtitle, new NSString("AdrsCell"), CellBindingText, UITableViewCellAccessory.None);

            TableView.Source = TableViewSource;
            TableViewSource.CellCreator = CellCreator;
                           

            AddSubviews(AddressEditText,CancelButton,TableView);
        }

        private MvxStandardTableViewCell CellCreator(UITableView tableView, NSIndexPath indexPath, object state)
        {
            var cell = new TwoLinesCell( new NSString("AdressCell"), CellBindingText,UITableViewCellAccessory.None );
            return cell;
        }

        private UIPanGestureRecognizer GetHideKeyboardOnTouchGesture()
        {
            return new HideKeyboardOnTouchGesture(AddressEditText);
        }

        public class HideKeyboardOnTouchGesture : UIPanGestureRecognizer
        {
            private UITextField _textField;

            public HideKeyboardOnTouchGesture( UITextField textField ) : base(  r=> r.ShouldRecognizeSimultaneously = (e,s) => true )
            {
                _textField = textField;

            }

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);
                if (_textField.IsFirstResponder)
                {
                    _textField.ResignFirstResponder();
                }
            }
        } 

        //Todo not sure about this
        protected void AddBindings(object source)
        {
//            this.AddBindings(source, new Dictionary<object, string>
//            {
//                { TableViewSource , "{'ItemsSource': {'Path': 'AllAddresses'}}" },
//                    { this , "{'SearchCommand': {'Path': 'TextSearchCommand'}}" },                
//                });
        }

        public void UpdateView(float width, float height)
        {
            Frame = new RectangleF(0, UIHelper.IsOS7orHigher ? 20:0, width, height);
            var top = CancelButton.Frame.Bottom + Margin2x;
            TableView.Frame = new RectangleF(0,top , width, height - top - Margin);

            var cancelButtonWidth = 90;
            CancelButton.Frame = new RectangleF(Frame.Width - cancelButtonWidth, Margin, cancelButtonWidth, 44);

            AddressEditText.SizeToFit();
            AddressEditText.Frame = new RectangleF(Margin, Margin, Frame.Width - CancelButton.Frame.Width - Margin, 44);
            AddressEditText.SetNeedsDisplay();
        }  

        void Close()
        {
            Hidden = true;
            this.ResignFirstResponderOnSubviews();
            UIView.Animate(0.4f, () => this.Alpha = 0);
        }


        public void Open()
        {
            this.Alpha = 0;
            UIView.Animate(0.4f, () => this.Alpha = 1);
            Hidden = false;

            Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(400);
                    InvokeOnMainThread(() => AddressEditText.BecomeFirstResponder());
                }
             );
        }        

        protected override void DrawStroke(MonoTouch.CoreGraphics.CGColor fillColor)
        {
            //nothing here, no shadow
        }

    }
}

