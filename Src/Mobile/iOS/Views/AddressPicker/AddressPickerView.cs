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
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views.AddressPicker
{
    [Register("AddressPickerView")]
    public class AddressPickerView : BaseBindableChildView<AddressPickerViewModel>
    {
        const string CellBindingText = @"
                   FirstLine DisplayLine1;
                   SecondLine DisplayLine2;
                   Icon Icon;
                   HideBottomBar IsLast";

        const float Margin = 8;
        const float Margin2x = Margin * 2;

        public AddressPickerView(IntPtr h) : base(h)
        {
            Initialize();
        }

        FlatTextField AddressEditText { get; set; }
        FlatButton CancelButton { get; set;}
        public GroupedAddressTableViewSource TableViewSource { get; set; }
        UITableView TableView {get; set;}

        void Initialize()
        {
            BackgroundColor = UIColor.FromRGB(242, 242, 242);
            Hidden = true;

            AddressEditText = new FlatTextField 
            { 
                ShowShadow = true,
                Frame = new RectangleF(Margin, 22, 214, 44),
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No
            };

            CancelButton = new FlatButton() { Frame = new RectangleF(AddressEditText.Frame.Right + 9, AddressEditText.Frame.Y, UIScreen.MainScreen.Bounds.Width - AddressEditText.Frame.Width - 9 - Margin2x, 44) };
            CancelButton.SetTitle(Localize.GetValue("Cancel"), UIControlState.Normal);
            FlatButtonStyle.Red.ApplyTo(CancelButton);

            var yPositionForTableView = AddressEditText.Frame.Bottom + 13;

            TableView = new UITableView(new RectangleF(0, yPositionForTableView, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - yPositionForTableView), UITableViewStyle.Grouped);
            TableView.SectionHeaderHeight = 15;
            TableView.BackgroundView = new  UIView { BackgroundColor = UIColor.Clear };
            TableView.BackgroundColor = UIColor.Clear;
            TableView.RowHeight = 44;
            TableView.AddGestureRecognizer(GetHideKeyboardOnTouchGesture());
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            TableViewSource = new GroupedAddressTableViewSource(TableView, 
                                                                UITableViewCellStyle.Subtitle, 
                                                                new NSString("AdrsCell"), 
                                                                CellBindingText, 
                                                                UITableViewCellAccessory.None);

            TableView.Source = TableViewSource;
            TableViewSource.CellCreator = CellCreator;
              
            AddSubviews(AddressEditText, CancelButton, TableView);

            AddressEditText.OnKeyDown()
                .Throttle(TimeSpan.FromMilliseconds(700))
                .Subscribe(text => ViewModel.TextSearchCommand.Execute(text));

            var set = this.CreateBindingSet<AddressPickerView, AddressPickerViewModel> ();

            set.Bind(TableViewSource)
                .For(v => v.ItemsSource)
                .To(vm => vm.AllAddresses);

            set.Bind(TableViewSource)
                .For(v => v.SelectedCommand)
                .To(vm => vm.AddressSelected);

            set.Bind(AddressEditText)
                .For(v => v.Text)
                .To(vm => vm.StartingText);

            set.Bind(CancelButton)
                .For("TouchUpInside")
                .To(vm => vm.Cancel);

            set.Apply ();
        }

        private MvxStandardTableViewCell CellCreator(UITableView tableView, NSIndexPath indexPath, object state)
        {
            var cell = new TwoLinesCell( new NSString("AdrsCell"), CellBindingText, UITableViewCellAccessory.None );
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

        public void Close()
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

        protected override void DrawBackground(MonoTouch.CoreGraphics.CGContext context, RectangleF rect, UIBezierPath roundedRectanglePath, MonoTouch.CoreGraphics.CGColor fillColor)
        {
            //nothing here, we don't want the semitransparent background
        }
    }
}

