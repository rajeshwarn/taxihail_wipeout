using System;
using Foundation;
using UIKit;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AppBarView")]
    public class AppBarView : MvxView, IChangePresentation
    {
        private UIView _reviewButtons;
        private UIView _orderButtons;
        private UIView _editButtons;

		// Keeping a reference to the _imagePromo object to ensure binding does not break.
		private UIImageView _imagePromo;

        public static CGSize ButtonSize = new CGSize(60, 46);

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
                BackgroundColor = UIColor.FromRGB(140, 140, 140)
            };

            AddSubview(Line);

            CreateButtonsForConfirmation();
            AddButtonsForBooking();
            CreateButtonsForEdit();
        }

        private void AddButtonsForBooking()
        {
            // - Estimate - Book Now - Book Later 

            _orderButtons = new UIView();
            _orderButtons.TranslatesAutoresizingMaskIntoConstraints = false;
            Add(_orderButtons);

			var btnEstimate = new AppBarButton(Localize.GetValue("Destination"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "destination_small_icon.png", "destination_small_icon_pressed.png");
            btnEstimate.TranslatesAutoresizingMaskIntoConstraints = false;

            var btnBook = new FlatButton();
            btnBook.TranslatesAutoresizingMaskIntoConstraints = false;
            FlatButtonStyle.Green.ApplyTo(btnBook);
            btnBook.SetTitle(Localize.GetValue("BookItButton"), UIControlState.Normal);

			_imagePromo = new UIImageView(UIImage.FromFile("promo.png"))
			{
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			_imagePromo.SetHeight(10f);
			_imagePromo.SetWidth(10f);
			btnBook.AddSubview(_imagePromo);

            var btnBookLater = new AppBarButton(Localize.GetValue("BookItLaterButton"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "later_icon.png", "later_icon_pressed.png");
            btnBookLater.TranslatesAutoresizingMaskIntoConstraints = false;

			_orderButtons.AddSubviews(btnEstimate, btnBook, btnBookLater);

            // Constraints for Container
            _orderButtons.Superview.AddConstraints(new []
            {
                NSLayoutConstraint.Create(_orderButtons, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _orderButtons.Superview, NSLayoutAttribute.Leading, 1, 0f),
                NSLayoutConstraint.Create(_orderButtons, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _orderButtons.Superview, NSLayoutAttribute.Top, 1, 0f),
                NSLayoutConstraint.Create(_orderButtons, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _orderButtons.Superview, NSLayoutAttribute.Width, 1, 0f),
                NSLayoutConstraint.Create(_orderButtons, NSLayoutAttribute.Height, NSLayoutRelation.Equal, _orderButtons.Superview, NSLayoutAttribute.Height, 1, 0f),
            });

            // Constraints for Estimate button
            _orderButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnEstimate, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.Leading, 1, 8f),
                NSLayoutConstraint.Create(btnEstimate, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, AppBarView.ButtonSize.Width),
                NSLayoutConstraint.Create(btnEstimate, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, AppBarView.ButtonSize.Height),
                NSLayoutConstraint.Create(btnEstimate, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.CenterY, 1, -4f),
            });

            // Constraints for Book Now button
            AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnBook, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.CenterX, 1, 0f),
                NSLayoutConstraint.Create(btnBook, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.CenterY, 1, 0f),
                NSLayoutConstraint.Create(btnBook, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _reviewButtons.Subviews[1], NSLayoutAttribute.Width, 1, 0f),
                NSLayoutConstraint.Create(btnBook, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 41f),
            });

			// Constraints for Image Promo button
			btnBook.AddConstraints(new []
			{
				NSLayoutConstraint.Create(_imagePromo, NSLayoutAttribute.Right, NSLayoutRelation.Equal, btnBook, NSLayoutAttribute.Right, 1, 0f),
				NSLayoutConstraint.Create(_imagePromo, NSLayoutAttribute.Top, NSLayoutRelation.Equal, btnBook, NSLayoutAttribute.Top, 1, 0f),
			});

            // Constraints for Book Later button
            _orderButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnBookLater, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.Trailing, 1, -8f),
                NSLayoutConstraint.Create(btnBookLater, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, AppBarView.ButtonSize.Width),
                NSLayoutConstraint.Create(btnBookLater, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, AppBarView.ButtonSize.Height),
                NSLayoutConstraint.Create(btnBookLater, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.CenterY, 1, -4f),
            });

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnEstimate)
                .For(v => v.Command)
                .To(vm => vm.ChangeAddressSelectionMode);
			
            set.Bind(btnEstimate)
                .For(v => v.Selected)
                .To(vm => vm.EstimateSelected);
			
			set.Bind(btnEstimate)
				.For(v => v.Hidden)
				.To(vm => vm.Settings.HideDestination);

            set.Bind(btnBook)
                .For(v => v.Command)
                .To(vm => vm.SetPickupDateAndReviewOrder);

			set.Bind(_imagePromo)
				.For(v => v.Hidden)
				.To(vm => vm.IsPromoCodeActive)
				.WithConversion("BoolInverter");

            set.Bind(btnBookLater)
                .For(v => v.Command)
                .To(vm => vm.BookLater);
			
            set.Bind(btnBookLater)
                .For(v => v.Hidden)
                .To(vm => vm.Settings.DisableFutureBooking);

            set.Apply();
        }

        private void CreateButtonsForConfirmation()
        {
            // - Cancel - Confirm - Edit 

            _reviewButtons = new UIView() { Hidden = true };
            _reviewButtons.TranslatesAutoresizingMaskIntoConstraints = false;
            Add(_reviewButtons);

			var btnCancel = new AppBarLabelButton(Localize.GetValue("Cancel"));
            btnCancel.TranslatesAutoresizingMaskIntoConstraints = false;

			var btnEdit = new AppBarLabelButton(Localize.GetValue("Edit"));
            btnEdit.TranslatesAutoresizingMaskIntoConstraints = false;

            var btnConfirm = new FlatButton();
            btnConfirm.TranslatesAutoresizingMaskIntoConstraints = false;
            FlatButtonStyle.Green.ApplyTo(btnConfirm);
			btnConfirm.SetTitle(Localize.GetValue("Confirm"), UIControlState.Normal);

            _reviewButtons.AddSubviews(btnCancel, btnConfirm, btnEdit);

            // Constraints for Container
            _reviewButtons.Superview.AddConstraints(new []
            {
                NSLayoutConstraint.Create(_reviewButtons, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _reviewButtons.Superview, NSLayoutAttribute.Leading, 1, 0f),
                NSLayoutConstraint.Create(_reviewButtons, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _reviewButtons.Superview, NSLayoutAttribute.Top, 1, 0f),
                NSLayoutConstraint.Create(_reviewButtons, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _reviewButtons.Superview, NSLayoutAttribute.Width, 1, 0f),
                NSLayoutConstraint.Create(_reviewButtons, NSLayoutAttribute.Height, NSLayoutRelation.Equal, _reviewButtons.Superview, NSLayoutAttribute.Height, 1, 0f),
            });

            // Constraints for Cancel button
            _reviewButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.Leading, 1, 8f),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 70f),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.CenterY, 1, 0),
            });

            // Constraints for Confirm button
            _reviewButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, btnCancel, NSLayoutAttribute.Trailing, 1, 20f),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, btnEdit, NSLayoutAttribute.Leading, 1, -20f),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 41),
            });

            // Constraints for Edit button
            _reviewButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnEdit, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.Trailing, 1, -8f),
                NSLayoutConstraint.Create(btnEdit, NSLayoutAttribute.Width, NSLayoutRelation.Equal, btnCancel, NSLayoutAttribute.Width, 1, 0f),
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
        }

        private void CreateButtonsForEdit()
        {
            // - Cancel - Save --------- 

            _editButtons = new UIView() { Hidden = true };
            _editButtons.TranslatesAutoresizingMaskIntoConstraints = false;
            Add(_editButtons);

			var btnCancel = new AppBarLabelButton(Localize.GetValue("Cancel"));
            btnCancel.TranslatesAutoresizingMaskIntoConstraints = false;

            var btnSave = new FlatButton();
            btnSave.TranslatesAutoresizingMaskIntoConstraints = false;
            FlatButtonStyle.Green.ApplyTo(btnSave);
			btnSave.SetTitle(Localize.GetValue("Save"), UIControlState.Normal);

            _editButtons.AddSubviews(btnCancel, btnSave);

            // Constraints for Container
            _editButtons.Superview.AddConstraints(new []
            {
                NSLayoutConstraint.Create(_editButtons, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _editButtons.Superview, NSLayoutAttribute.Leading, 1, 0f),
                NSLayoutConstraint.Create(_editButtons, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _editButtons.Superview, NSLayoutAttribute.Top, 1, 0f),
                NSLayoutConstraint.Create(_editButtons, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _editButtons.Superview, NSLayoutAttribute.Width, 1, 0f),
                NSLayoutConstraint.Create(_editButtons, NSLayoutAttribute.Height, NSLayoutRelation.Equal, _editButtons.Superview, NSLayoutAttribute.Height, 1, 0f),
            });

            // Constraints for Cancel button
            _editButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.Leading, 1, 8f),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 70f),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.CenterY, 1, 0),
            });

            // Constraints for Save button
            AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnSave, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(btnSave, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(btnSave, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _reviewButtons.Subviews[1], NSLayoutAttribute.Width, 1, 0f),
                NSLayoutConstraint.Create(btnSave, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 41),
            });

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.CancelEdit);

            set.Bind(btnSave)
                .For(v => v.Command)
                .To(vm => vm.Save);

            set.Apply();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (Line != null)
            {
                Line.Frame = new CGRect(0, 0, Frame.Width, UIHelper.OnePixel);
            }
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

