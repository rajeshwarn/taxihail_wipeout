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
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AppBarView")]
    public class AppBarView : MvxView
    {
        private UIView _reviewButtons;
        private UIView _orderButtons;
        private UIView _editButtons;
		private UIView _manualPairingButtons;
		private UIView _airportOrderButtons;

		// Keeping a reference to the _imagePromo object to ensure binding does not break.
		private UIImageView _imagePromo;
		private UIImageView _imagePromoForManual;

        public static CGSize ButtonSize = new CGSize(60, 46);

        private const float ContainerHorizontalPadding = 8f;
        private const float TextButtonWidth = 70f;
        private const float CenterButtonHorizontalPadding = 20f;
        private const float ButtonHeight = 41f;

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
			CreateButtonsForManualRideLinqFlow();
			CreateButtonsForAirportBooking ();
        }

		private void CreateButtonsForManualRideLinqFlow()
		{
			_manualPairingButtons = new UIView();
			_manualPairingButtons.TranslatesAutoresizingMaskIntoConstraints = false;
			Add(_manualPairingButtons);

            bool hideDestination = Mvx.Resolve<IAppSettings>().Data.HideDestination;

            AppBarButton btnEstimate = null;

			if (!hideDestination)
			{
				btnEstimate = GenerateEstimateButton();
			}

			var btnBookForManualRideLinq = GenerateBookButton();
            
			_imagePromoForManual = GeneratePromoImage();
			btnBookForManualRideLinq.AddSubview(_imagePromoForManual);

			var btnManual = new FlatButton()
			{
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			FlatButtonStyle.Blue.ApplyTo(btnManual);
			btnManual.SetTitle(Localize.GetValue("HomeView_ManualPairing"), UIControlState.Normal);

            if (!hideDestination)
            {
                _manualPairingButtons.AddSubviews(btnEstimate);
            }

            _manualPairingButtons.AddSubviews(btnManual, btnBookForManualRideLinq);

            if (!hideDestination)
            {
                _manualPairingButtons.AddConstraints(GenerateEstimateButtonConstraints(btnEstimate, _manualPairingButtons));
            }

			btnBookForManualRideLinq.AddConstraints(GenerateImagePromoConstraints(_imagePromoForManual, btnBookForManualRideLinq));

			_manualPairingButtons.Superview.AddConstraints(GenerateConstraintsForContainer(_manualPairingButtons));


            // Constraints for Manual button
            AddConstraints(new []
                {
                    NSLayoutConstraint.Create(btnManual, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, ButtonHeight),
                    NSLayoutConstraint.Create(btnManual, NSLayoutAttribute.Width, NSLayoutRelation.Equal, btnBookForManualRideLinq, NSLayoutAttribute.Width, 1, 0f),
                    NSLayoutConstraint.Create(btnManual, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _manualPairingButtons, NSLayoutAttribute.CenterY, 1, 0f),
                    NSLayoutConstraint.Create(btnBookForManualRideLinq, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, btnManual, NSLayoutAttribute.Trailing, 1, 10f),
                });
            
            if (!hideDestination)
            {
                AddConstraints(new []
                    {
                        NSLayoutConstraint.Create(btnManual, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, btnEstimate, NSLayoutAttribute.Trailing, 1, 10f),
                    });
            }
            else
            {
                AddConstraints(new []
                    {
                        NSLayoutConstraint.Create(btnManual, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _manualPairingButtons, NSLayoutAttribute.Leading, 1, 10f),
                    }); 
            }

			// Constraints for Book Now button
			AddConstraints(new []
			{
				NSLayoutConstraint.Create(btnBookForManualRideLinq, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _manualPairingButtons, NSLayoutAttribute.Trailing, 1, -10f),
				NSLayoutConstraint.Create(btnBookForManualRideLinq, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _manualPairingButtons, NSLayoutAttribute.CenterY, 1, 0f),
                NSLayoutConstraint.Create(btnBookForManualRideLinq, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, ButtonHeight),
			});


			var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();


            if (!hideDestination)
            {
                set.Bind(btnEstimate)
				.For(v => v.Command)
				.To(vm => vm.ChangeAddressSelectionMode);

                set.Bind(btnEstimate)
				.For(v => v.Selected)
				.To(vm => vm.EstimateSelected);

                set.Bind(btnEstimate)
                .For(v => v.Hidden)
				.To(vm => vm.Settings.HideDestination);
            }

			set.Bind(btnBookForManualRideLinq)
				.For(v => v.Command)
				.To(vm => vm.Book);
            
		    set.Bind(btnBookForManualRideLinq)
		        .For("Title")
		        .To(vm => vm.BookButtonText);

            set.Bind(_manualPairingButtons)
                .For(v => v.Hidden)
                .To(vm => vm.HideManualRideLinqButtons);

			set.Bind(_imagePromoForManual)
				.For(v => v.Hidden)
				.To(vm => vm.IsPromoCodeActive)
				.WithConversion("BoolInverter");

			set.Bind(btnManual)
				.For(v => v.Command)
				.To(vm => vm.ManualPairingRideLinq);

			set.Apply();
		}

		private AppBarButton GenerateEstimateButton()
		{
			return new AppBarButton(Localize.GetValue("Destination"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "destination_small_icon.png", "destination_small_icon_pressed.png")
			{
				TranslatesAutoresizingMaskIntoConstraints = false
			};
		}

		private FlatButton GenerateBookButton()
		{
            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            var btnBook = new FlatButton()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
            FlatButtonStyle.Green.ApplyTo(btnBook);

            set.Bind(btnBook).For(v => v.Hidden).To(vm => vm.BookButtonHidden);
            set.Apply();

            return btnBook;
		}

		private UIImageView GeneratePromoImage()
		{
			var imagePromo =new UIImageView(UIImage.FromFile("promo.png"))
			{
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			imagePromo.SetHeight(10f);
			imagePromo.SetWidth(10f);

			return imagePromo;
		}

		private NSLayoutConstraint[] GenerateEstimateButtonConstraints(UIView btnEstimate, UIView container)
		{
			return new []
			{
                NSLayoutConstraint.Create(btnEstimate, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, container, NSLayoutAttribute.Leading, 1, ContainerHorizontalPadding),
				NSLayoutConstraint.Create(btnEstimate, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, AppBarView.ButtonSize.Width),
				NSLayoutConstraint.Create(btnEstimate, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, AppBarView.ButtonSize.Height),
				NSLayoutConstraint.Create(btnEstimate, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, container, NSLayoutAttribute.CenterY, 1, -4f),
			};
		}

		private NSLayoutConstraint[] GenerateImagePromoConstraints(UIView imagePromo, UIView container)
		{
			return new []
			{
				NSLayoutConstraint.Create(imagePromo, NSLayoutAttribute.Right, NSLayoutRelation.Equal, container, NSLayoutAttribute.Right, 1, 0f),
				NSLayoutConstraint.Create(imagePromo, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container, NSLayoutAttribute.Top, 1, 0f),
			};
		}

		private NSLayoutConstraint[] GenerateConstraintsForContainer(UIView container)
		{
			return new []
			{
				NSLayoutConstraint.Create(container, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, container.Superview, NSLayoutAttribute.Leading, 1, 0f),
				NSLayoutConstraint.Create(container, NSLayoutAttribute.Top, NSLayoutRelation.Equal, container.Superview, NSLayoutAttribute.Top, 1, 0f),
				NSLayoutConstraint.Create(container, NSLayoutAttribute.Width, NSLayoutRelation.Equal, container.Superview, NSLayoutAttribute.Width, 1, 0f),
				NSLayoutConstraint.Create(container, NSLayoutAttribute.Height, NSLayoutRelation.Equal, container.Superview, NSLayoutAttribute.Height, 1, 0f),
			};
		}

        private void AddButtonsForBooking()
        {
            // - Estimate - Book Now - Book Later 

            _orderButtons = new UIView();
            _orderButtons.TranslatesAutoresizingMaskIntoConstraints = false;
            Add(_orderButtons);

			var btnEstimate = GenerateEstimateButton();

			var btnBook = GenerateBookButton();

			_imagePromo = GeneratePromoImage();
			btnBook.AddSubview(_imagePromo);

            var btnBookLater = new AppBarButton(Localize.GetValue("BookItLaterButton"), AppBarView.ButtonSize.Width, AppBarView.ButtonSize.Height, "later_icon.png", "later_icon_pressed.png");
            btnBookLater.TranslatesAutoresizingMaskIntoConstraints = false;

			_orderButtons.AddSubviews(btnEstimate, btnBook, btnBookLater);

            // Constraints for Container
			_orderButtons.Superview.AddConstraints(GenerateConstraintsForContainer(_orderButtons));

            // Constraints for Estimate button
			_orderButtons.AddConstraints(GenerateEstimateButtonConstraints(btnEstimate, _orderButtons));

            // Constraints for Book Now button
            AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnBook, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.CenterX, 1, 0f),
                NSLayoutConstraint.Create(btnBook, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.CenterY, 1, 0f),
                NSLayoutConstraint.Create(btnBook, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _reviewButtons.Subviews[1], NSLayoutAttribute.Width, 1, 0f),
                NSLayoutConstraint.Create(btnBook, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, ButtonHeight),
            });

			// Constraints for Image Promo button
			btnBook.AddConstraints(GenerateImagePromoConstraints(_imagePromo, btnBook));

            // Constraints for Book Later button
            _orderButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnBookLater, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _orderButtons, NSLayoutAttribute.Trailing, 1, -ContainerHorizontalPadding),
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
                .To(vm => vm.Book);
            
			set.Bind(btnBook)
				.For("Title")
				.To(vm => vm.BookButtonText);

			set.Bind(_imagePromo)
				.For(v => v.Hidden)
				.To(vm => vm.IsPromoCodeActive)
				.WithConversion("BoolInverter");

            set.Bind(btnBookLater)
                .For(v => v.Command)
                .To(vm => vm.BookLater);
			
            set.Bind(btnBookLater)
                .For(v => v.Hidden)
                .To(vm => vm.IsFutureBookingDisabled);

            set.Bind(_orderButtons)
                .For(v => v.Hidden)
                .To(vm => vm.HideOrderButtons);

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
            _reviewButtons.Superview.AddConstraints(GenerateConstraintsForContainer(_reviewButtons));

            // Constraints for Cancel button
            _reviewButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.Leading, 1, ContainerHorizontalPadding),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, TextButtonWidth),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.CenterY, 1, 0),
            });

            // Constraints for Confirm button
            _reviewButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, btnCancel, NSLayoutAttribute.Trailing, 1, CenterButtonHorizontalPadding),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, btnEdit, NSLayoutAttribute.Leading, 1, -CenterButtonHorizontalPadding),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, ButtonHeight),
            });

            // Constraints for Edit button
            _reviewButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnEdit, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.Trailing, 1, -ContainerHorizontalPadding),
                NSLayoutConstraint.Create(btnEdit, NSLayoutAttribute.Width, NSLayoutRelation.Equal, btnCancel, NSLayoutAttribute.Width, 1, 0f),
                NSLayoutConstraint.Create(btnEdit, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _reviewButtons, NSLayoutAttribute.CenterY, 1, 0),
            });

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.CancelReview);

            set.Bind(btnConfirm)
                .For(v => v.Command)
                .To(vm => vm.ConfirmOrderCommand);

            set.Bind(btnEdit)
                .For(v => v.Command)
                .To(vm => vm.Edit);

            set.Bind(_reviewButtons)
                .For(v => v.Hidden)
                .To(vm => vm.HideReviewButtons);

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
            _editButtons.Superview.AddConstraints(GenerateConstraintsForContainer(_editButtons));

            // Constraints for Cancel button
            _editButtons.AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.Leading, 1, ContainerHorizontalPadding),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, TextButtonWidth),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.CenterY, 1, 0),
            });

            // Constraints for Save button
            AddConstraints(new []
            {
                NSLayoutConstraint.Create(btnSave, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(btnSave, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _editButtons, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(btnSave, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _reviewButtons.Subviews[1], NSLayoutAttribute.Width, 1, 0f),
                NSLayoutConstraint.Create(btnSave, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, ButtonHeight),
            });

            var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

            set.Bind(btnCancel)
                .For(v => v.Command)
                .To(vm => vm.CancelEdit);

            set.Bind(btnSave)
                .For(v => v.Command)
                .To(vm => vm.Save);

            set.Bind(_editButtons)
                .For(v => v.Hidden)
                .To(vm => vm.HideEditButtons);
            
            set.Apply();
        }

		private void CreateButtonsForAirportBooking()
		{
			// - Cancel - Confirm
			_airportOrderButtons = new UIView() { Hidden = true };
			_airportOrderButtons.TranslatesAutoresizingMaskIntoConstraints = false;
			Add(_airportOrderButtons);

			var btnCancel = new AppBarLabelButton(Localize.GetValue("Cancel"));
			btnCancel.TranslatesAutoresizingMaskIntoConstraints = false;

			var btnConfirm = new FlatButton();
			btnConfirm.TranslatesAutoresizingMaskIntoConstraints = false;
			FlatButtonStyle.Green.ApplyTo(btnConfirm);
			btnConfirm.SetTitle(Localize.GetValue("Next"), UIControlState.Normal);

            _airportOrderButtons.AddSubviews(btnCancel, btnConfirm);

			// Constraints for Container
            _airportOrderButtons.Superview.AddConstraints(GenerateConstraintsForContainer(_airportOrderButtons));

            // Constraints for Cancel button
            _airportOrderButtons.AddConstraints(new [] 
            {
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _airportOrderButtons, NSLayoutAttribute.Leading, 1, ContainerHorizontalPadding),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, TextButtonWidth),
                NSLayoutConstraint.Create(btnCancel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _airportOrderButtons, NSLayoutAttribute.CenterY, 1, 0)
            });

			// Constraints for Confirm button
			_airportOrderButtons.AddConstraints(new []
			{
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _airportOrderButtons, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, btnCancel, NSLayoutAttribute.Trailing, 1, CenterButtonHorizontalPadding),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _airportOrderButtons, NSLayoutAttribute.Trailing, 1, -(CenterButtonHorizontalPadding + ContainerHorizontalPadding + TextButtonWidth)),
                NSLayoutConstraint.Create(btnConfirm, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, ButtonHeight)
			});

			var set = this.CreateBindingSet<AppBarView, BottomBarViewModel>();

			set.Bind(btnCancel)
				.For(v => v.Command)
                .To(vm => vm.CancelAirport);

			set.Bind(btnConfirm)
				.For(v => v.Command)
				.To(vm => vm.NextAirport);

            set.Bind(_airportOrderButtons)
                .For(v => v.Hidden)
                .To(vm => vm.HideAirportButtons);

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
	}
}