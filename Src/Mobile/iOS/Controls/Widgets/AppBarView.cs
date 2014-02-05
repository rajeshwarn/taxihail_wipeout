using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Booking;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AppBarView")]
    public class AppBarView : MvxView
    {
        private UIView _confirmationButtons;
        private UIView _orderButtons;
        public static SizeF ButtonSize = new SizeF(60, 46);

        protected UIView Line { get; set; }

        public AppBarView (IntPtr ptr):base(ptr)
        {
            Initialize ();            
        }

        public AppBarView ()
        {
            Initialize ();
        }

        void Initialize ()
        {
            BackgroundColor = UIColor.White;

            Line = new UIView()
            {
                Frame = new RectangleF(0, 0, Frame.Width, UIHelper.OnePixel),
                BackgroundColor = UIColor.FromRGB(140, 140, 140)
            };

            AddSubview(Line);

            AddButtonsForBooking();
            CreateButtonsForConfirmation();
        }

        private void AddButtonsForBooking()
        {
            _orderButtons = new UIView(this.Bounds);

            var btnEstimate = new AppBarButton(Localize.GetValue("Estimate"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "estimate_icon.png", "estimate_icon_pressed.png");
            btnEstimate.Frame = btnEstimate.Frame.IncrementX(4);

            var btnBook = new FlatButton(new RectangleF((320 - 123)/2, 7, 123, 41));
            FlatButtonStyle.Green.ApplyTo(btnBook);
            btnBook.SetTitle(Localize.GetValue("BookItButton"), UIControlState.Normal);

            var _bookLaterDatePicker = new BookLaterDatePicker();            
            _bookLaterDatePicker.UpdateView(Superview.Frame.Height, Superview.Frame.Width);
            _bookLaterDatePicker.Hide();
            Superview.AddSubview(_bookLaterDatePicker);

            var btnBookLater = new AppBarButton(Localize.GetValue("BookItLaterButton"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "later_icon.png", "later_icon_pressed.png");
            btnBookLater.Frame = btnBookLater.Frame.SetX(Frame.Width - btnBookLater.Frame.Width - 3);
            btnBookLater.TouchUpInside += (sender, e) => _bookLaterDatePicker.Show();

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnEstimate)
                .For(v => v.Command)
                .To(vm => vm.ChangeAddressSelectionMode);
            set.Bind(btnEstimate)
                .For(v => v.Selected)
                .To(vm => vm.EstimateSelected);

            set.Bind(_bookLaterDatePicker)
                .For(v => v.DataContext)
                .To(vm => vm.BookLater);

            set.Bind(btnBook)
                .For("TouchUpInside")
                .To(vm => vm.BookNow);

            set.Apply();

            _orderButtons.AddSubviews(btnEstimate, btnBook, btnBookLater);
            Add(_orderButtons);
        }

        private void CreateButtonsForConfirmation()
        {
            _confirmationButtons = new UIView(this.Bounds);

            var btnCancel = new AppBarLabelButton("Cancel");
            btnCancel.TranslatesAutoresizingMaskIntoConstraints = false;

            //btnCancel.UpdateView(10, 10, 100, 41);

            var btnConfirm = new FlatButton(new RectangleF((320 - 123)/2, 7, 123, 41));
            FlatButtonStyle.Green.ApplyTo(btnConfirm);
            btnConfirm.SetTitle(Localize.GetValue("ConfirmButton"), UIControlState.Normal);

            _confirmationButtons.AddSubviews(btnCancel, btnConfirm);

            _confirmationButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _confirmationButtons, NSLayoutAttribute.Leading, 1, 5f),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _confirmationButtons, NSLayoutAttribute.CenterY, 1, 0),
            });

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.Cancel);

            set.Bind(btnConfirm)
                .For(v => v.Command)
                .To(vm => vm.ConfirmOrder);

            set.Apply();
        }

        public void ChangeState(OrderReviewPresentationHint hint)
        {
            foreach(var subview in Subviews)
            {
                subview.RemoveFromSuperview();
            }
            if (hint.Show)
            {
                Add(_confirmationButtons);
            }
            else
            {
                Add(_orderButtons);
            }
        }
    }
}

