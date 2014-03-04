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
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AppBarView")]
    public class AppBarView : MvxView, IChangePresentation
    {
        private UIView _reviewButtons;
        private UIView _orderButtons;
        private UIView _editButtons;
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
            CreateButtonsForEdit();
        }

        private void AddButtonsForBooking()
        {
            // - Menu - Book Now - Book Later 

            _orderButtons = new UIView(this.Bounds);

            var btnEstimate = new AppBarButton(Localize.GetValue("Estimate"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "estimate_icon.png", "estimate_icon_pressed.png");
            btnEstimate.Frame = btnEstimate.Frame.IncrementX(4);

            var btnBook = new FlatButton(new RectangleF((320 - 123)/2, 7, 123, 41));
            FlatButtonStyle.Green.ApplyTo(btnBook);
            btnBook.SetTitle(Localize.GetValue("BookItButton"), UIControlState.Normal);

            var btnBookLater = new AppBarButton(Localize.GetValue("BookItLaterButton"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "later_icon.png", "later_icon_pressed.png");
            btnBookLater.Frame = btnBookLater.Frame.SetX(Frame.Width - btnBookLater.Frame.Width - 3);

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnEstimate)
                .For(v => v.Command)
                .To(vm => vm.ChangeAddressSelectionMode);
            set.Bind(btnEstimate)
                .For(v => v.Selected)
                .To(vm => vm.EstimateSelected);

            set.Bind(btnBook)
                .For(v => v.Command)
                .To(vm => vm.SetPickupDateAndReviewOrder);

            set.Bind(btnBookLater)
                .For(v => v.Command)
                .To(vm => vm.BookLater);

            set.Bind(btnBookLater)
                .For(v => v.Hidden)
                .To(vm => vm.DisableFutureBooking);

            set.Apply();

            _orderButtons.AddSubviews(btnEstimate, btnBook, btnBookLater);
            Add(_orderButtons);
        }

        private void CreateButtonsForConfirmation()
        {
            // - Cancel - Confirm - Edit 

            _reviewButtons = new UIView(this.Bounds) { Hidden = true };

            var btnCancel = new AppBarLabelButton("Cancel");
            btnCancel.TranslatesAutoresizingMaskIntoConstraints = false;

            var btnEdit = new AppBarLabelButton("Edit");
            btnEdit.TranslatesAutoresizingMaskIntoConstraints = false;

            var btnConfirm = new FlatButton(new RectangleF((320 - 123)/2, 7, 123, 41));
            FlatButtonStyle.Green.ApplyTo(btnConfirm);
            btnConfirm.SetTitle("Confirm", UIControlState.Normal);

            _reviewButtons.AddSubviews(btnCancel, btnConfirm, btnEdit);

            // Constraints for Cancel button
            _reviewButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.Leading, 1, 8f),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.CenterY, 1, 0),
            });

            // Constraints for Edit button
            _reviewButtons.AddConstraints(new []
                {
                    NSLayoutConstraint.Create(btnEdit, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.Trailing, 1, -8f),
                    NSLayoutConstraint.Create(btnEdit, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.CenterY, 1, 0),
                });

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.CancelReview);

            set.Bind(btnConfirm)
                .For(v => v.Command)
                .To(vm => vm.ConfirmOrder);

            set.Bind(btnEdit)
                .For(v => v.Command)
                .To(vm => vm.Edit);

            set.Apply();

            Add(_reviewButtons);
        }

        private void CreateButtonsForEdit()
        {
            // - Cancel - Save --------- 

            _editButtons = new UIView(this.Bounds) { Hidden = true };

            var btnCancel = new AppBarLabelButton("Cancel");
            btnCancel.TranslatesAutoresizingMaskIntoConstraints = false;

            var btnSave = new FlatButton(new RectangleF((320 - 123)/2, 7, 123, 41));
            FlatButtonStyle.Green.ApplyTo(btnSave);
            btnSave.SetTitle("Save", UIControlState.Normal);

            _editButtons.AddSubviews(btnCancel, btnSave);

            // Constraints for Cancel button
            _editButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.Leading, 1, 8f),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.CenterY, 1, 0),
            });

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.CancelEdit);

            set.Bind(btnSave)
                .For(v => v.Command)
                .To(vm => vm.Save);

            set.Apply();

            Add(_editButtons);
        }

        private void ChangeState(HomeViewModelPresentationHint hint)
        {   
            if (hint.State == HomeViewModelState.PickDate)
            {
                // This state does not affect this control
                return;
            }
            _orderButtons.Hidden = hint.State != HomeViewModelState.Initial;
            _reviewButtons.Hidden = hint.State != HomeViewModelState.Review;
            _editButtons.Hidden = hint.State != HomeViewModelState.Edit;
        }

        void IChangePresentation.ChangePresentation(ChangePresentationHint hint)
        {
            if (hint is HomeViewModelPresentationHint)
            {
                ChangeState((HomeViewModelPresentationHint)hint);
            }
        }
    }
}

