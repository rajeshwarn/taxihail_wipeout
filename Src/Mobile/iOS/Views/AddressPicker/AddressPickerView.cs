using System;
using Foundation;
using UIKit;
using CoreGraphics;
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
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Views.AddressPicker
{
    [Register("AddressPickerView")]
    public class AddressPickerView : BaseBindableChildView<AddressPickerViewModel>
    {
        const string CellId = "AdrsCell";
        const string CellBindingText = @"
                   FirstLine DisplayLine1;
                   SecondLine DisplayLine2;
                   Icon Icon;
                   HideBottomBar IsLast";

//        const float Margin = 8;
//        const float Margin2x = Margin * 2;

        public AddressPickerView(IntPtr h) : base(h)
        {
            Initialize();
        }

        FlatTextField AddressEditText { get; set; }
        FlatButton CancelButton { get; set;}
        UITableView TableView {get; set;}

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var yPositionForTableView = AddressEditText.Frame.Bottom + 13;
            TableView.Frame = new CGRect(0, yPositionForTableView, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - yPositionForTableView);
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Clear;
            Hidden = true;

            AddressEditText = new FlatTextField 
            { 
                ShowShadow = true,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No
            };

            AddressEditText.Placeholder = Localize.GetValue("PickupTextPlaceholder");
            AddressEditText.AccessibilityLabel = AddressEditText.Placeholder;

            AddressEditText.TranslatesAutoresizingMaskIntoConstraints = false;

            CancelButton = new FlatButton();
            CancelButton.TranslatesAutoresizingMaskIntoConstraints = false;
            CancelButton.SetTitle(Localize.GetValue("Cancel"), UIControlState.Normal);
            FlatButtonStyle.Red.ApplyTo(CancelButton);

            TableView = new UITableView(new CGRect(), UITableViewStyle.Plain)
            {
                BackgroundView = new  UIView { BackgroundColor = UIColor.Clear },
                BackgroundColor = UIColor.FromRGB(242, 242, 242),
                SeparatorColor = UIColor.Clear,
                SeparatorStyle = UITableViewCellSeparatorStyle.None,
                RowHeight = 44,
                SectionHeaderHeight = 15
            };
            TableView.AddGestureRecognizer(GetHideKeyboardOnTouchGesture());

            var source = new GroupedAddressTableViewSource(
                TableView, 
                UITableViewCellStyle.Subtitle, 
                new NSString(CellId), 
                CellBindingText, 
                UITableViewCellAccessory.None
            );
            source.CellCreator = CellCreator;
            TableView.Source = source;

            AddSubviews(AddressEditText, CancelButton, TableView);

            AddressEditText.OnKeyDown()
                .Throttle(TimeSpan.FromMilliseconds(700))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(text => ViewModel.TextSearchCommand.ExecuteIfPossible(text));
				
            AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(AddressEditText, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1f, 8f),
                NSLayoutConstraint.Create(AddressEditText, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, UIHelper.IsOS7orHigher ? 22f : 5f),
                NSLayoutConstraint.Create(AddressEditText, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 44f),

                NSLayoutConstraint.Create(AddressEditText, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, CancelButton, NSLayoutAttribute.Leading, 1f, -9f),

                NSLayoutConstraint.Create(CancelButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1f, -8f),
                NSLayoutConstraint.Create(CancelButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, UIHelper.IsOS7orHigher ? 22f : 5f),
                NSLayoutConstraint.Create(CancelButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 44f),
                NSLayoutConstraint.Create(CancelButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 81f),
            });

            var set = this.CreateBindingSet<AddressPickerView, AddressPickerViewModel> ();

            set.Bind(source)
                .For(v => v.ItemsSource)
                .To(vm => vm.AllAddresses);

            set.Bind(source)
                .For(v => v.SelectedCommand)
                .To(vm => vm.AddressSelected);

            set.Bind(AddressEditText)
                .For(v => v.Text)
                .To(vm => vm.StartingText)
                .OneWay();

            set.Bind(CancelButton)
                .For("TouchUpInside")
                .To(vm => vm.Cancel);

            set.Apply ();

            SetNeedsLayout();
        }

        private MvxStandardTableViewCell CellCreator(UITableView tableView, NSIndexPath indexPath, object state)
        {
            var cell = new TwoLinesCell(new NSString(CellId), CellBindingText, UITableViewCellAccessory.None);
            cell.RemoveDelay();
            return cell;
        }

        private UIPanGestureRecognizer GetHideKeyboardOnTouchGesture()
        {
            return new HideKeyboardOnTouchGesture(AddressEditText);
        }

        public class HideKeyboardOnTouchGesture : UIPanGestureRecognizer
        {
            private UITextField _textField;

            public HideKeyboardOnTouchGesture( UITextField textField ) : base(r => r.ShouldRecognizeSimultaneously = (e,s) => true )
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
            this.ResignFirstResponderOnSubviews();
            UIView.Animate(0.3f, () => this.Alpha = 0, () => this.Hidden = true);
        }

		public async void Open(AddressLocationType addressLocationType)
        {
			Alpha = 0;
			Hidden = false;
			Animate(0.3f, () => Alpha = 1);

			await ViewModel.LoadAddresses(addressLocationType).HandleErrors();

			if (addressLocationType == AddressLocationType.Unspeficied)
			{
				FocusOnTextField();
			}
        }        

		public void FocusOnTextField()
		{
			Task.Factory.StartNew(() =>
			{
				InvokeOnMainThread(() => AddressEditText.BecomeFirstResponder());
			});
		}

        protected override void DrawStroke(CoreGraphics.CGColor fillColor, CGRect rect)
        {
            //nothing here, no shadow
        }

        protected override void DrawBackground(CoreGraphics.CGContext context, CGRect rect, UIBezierPath roundedRectanglePath, CoreGraphics.CGColor fillColor)
        {
            //nothing here, we don't want the semitransparent background
        }
    }
}

