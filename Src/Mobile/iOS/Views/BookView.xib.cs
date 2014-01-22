using System;
using System.Collections.Generic;
using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class BookView : MvxViewController
    {
        private PanelMenuView _menu;
        private DateTimePicker _dateTimePicker;
        private Action _onDateTimePicked;
        private BookViewActionsView _bottomAction;

        public BookView () 
			: base("BookView", null)
        {
        }

		public new BookViewModel ViewModel
		{
			get
			{
				return (BookViewModel)DataContext;
			}
		}

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            navBar.SetBackgroundImage (UIImage.FromFile ("Assets/navBar.png"), UIBarMetrics.Default);
			navBar.TopItem.Title = string.Empty;

            bookView.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
                       
            this.Services().MessengerHub.Subscribe<StatusCloseRequested> (OnStatusCloseRequested);
            this.Services().MessengerHub.Subscribe<DateTimePicked> (msg => _onDateTimePicked ());
            _dateTimePicker = new DateTimePicker (CultureProvider.CultureInfoString);
            _dateTimePicker.ShowPastDate = false;

            _onDateTimePicked = () => _dateTimePicker.Hide ();
            View.AddSubview (_dateTimePicker);
                       
            AppButtons.FormatStandardButton ((GradientButton)dropoffActivationButton, "", AppStyle.ButtonColor.LightBlue, "");
            AppButtons.FormatStandardButton ((GradientButton)pickupActivationButton, "", AppStyle.ButtonColor.LightBlue);

            ((GradientButton)dropoffActivationButton).SetImage ("Assets/flag.png");
            ((GradientButton)pickupActivationButton).SetImage ("Assets/hail.png");

            ((GradientButton)dropoffActivationButton).SetSelectedImage ("Assets/flag_selected.png");
            ((GradientButton)pickupActivationButton).SetSelectedImage ("Assets/hail_selected.png");

            headerBackgroundView.Image = UIImage.FromFile ("Assets/backPickupDestination.png");

            mapView.MultipleTouchEnabled = true;
            mapView.Delegate = new AddressMapDelegate ();

            bottomBar.UserInteractionEnabled = true;
            bookView.BringSubviewToFront (bottomBar);
            _bottomAction = new BookViewActionsView();
            bottomBar.Subviews.ForEach ( v=>v.Hidden = true );
            _bottomAction.Frame = new RectangleF( 0,0, bottomBar.Bounds.Width, bottomBar.Bounds.Height );
            bottomBar.AddSubview( _bottomAction );

            _bottomAction.BookLaterButton.TouchUpInside += delegate {
                ViewModel.Panel.MenuIsOpen = false;
                _dateTimePicker.Show (ViewModel.Order.PickupDate);
            };                      

            var set = this.CreateBindingSet<BookView, BookViewModel>();

            set.Bind(_bottomAction.RefreshCurrentLocationButton)
                .For("TouchUpInside")
                .To(vm =>  vm.SelectedAddress.RequestCurrentLocationCommand);

            set.Bind(pickupActivationButton)
                .For("TouchUpInside")
                .To(vm => vm.ActivatePickup);
            set.Bind(pickupActivationButton)
                .For(v => v.Selected)
                .To(vm => vm.AddressSelectionMode)
                .WithConversion("EnumToBool", "PickupSelection");

            set.Bind(dropoffActivationButton)
                .For("TouchUpInside")
                .To(vm => vm.ActivateDropoff);
            set.Bind(dropoffActivationButton)
                .For(v => v.Selected)
                .To(vm => vm.AddressSelectionMode)
                .WithConversion("EnumToBool", "DropoffSelection");
            set.Bind(dropoffActivationButton)
                .For(v => v.Hidden)
                .To(vm => vm.HideDestination);

            set.Bind(pickupButton)
                .For("TouchUpInside")
                .To(vm => vm.Pickup.PickAddress);
            set.Bind(pickupButton)
                .For(v => v.TextLine1)
                .To(vm => vm.Pickup.AddressLine1);
            set.Bind(pickupButton)
                .For(v => v.TextLine2)
                .To(vm => vm.Pickup.AddressLine2);
            set.Bind(pickupButton)
                .For(v => v.IsSearching)
                .To(vm => vm.Pickup.IsExecuting);
            set.Bind(pickupButton)
                .For(v => v.IsPlaceholder)
                .To(vm => vm.Pickup.IsPlaceHolder);

            set.Bind(dropoffButton)
                .For("TouchUpInside")
                .To(vm => vm.Dropoff.PickAddress);
            set.Bind(dropoffButton)
                .For(v => v.TextLine1)
                .To(vm => vm.Dropoff.AddressLine1);
            set.Bind(dropoffButton)
                .For(v => v.TextLine2)
                .To(vm => vm.Dropoff.AddressLine2);
            set.Bind(dropoffButton)
                .For(v => v.IsSearching)
                .To(vm => vm.Dropoff.IsExecuting);
            set.Bind(dropoffButton)
                .For(v => v.IsPlaceholder)
                .To(vm => vm.Dropoff.IsPlaceHolder);
            set.Bind(dropoffButton)
                .For(v => v.Hidden)
                .To(vm => vm.HideDestination);


            set.Bind(mapView)
                .For(v => v.Pickup)
                .To(vm => vm.Pickup.Model);
            set.Bind(mapView)
                .For(v => v.Dropoff)
                .To(vm => vm.Dropoff.Model);
            set.Bind(mapView)
                .For(v => v.MapMoved)
                .To(vm => vm.SelectedAddress.SearchCommand);
            set.Bind(mapView)
                .For(v => v.MapCenter)
                .To(vm => vm.MapCenter);
            set.Bind(mapView)
                .For("AvailableVehicles")
                .To(vm => vm.AvailableVehicles);
            set.Bind(mapView)
                .For(v => v.AddressSelectionMode)
                .To(vm => vm.AddressSelectionMode);

            set.Bind(infoLabel)
                .For(v => v.Text)
                .To(vm => vm.FareEstimate);
            set.Bind(infoLabel)
                .For(v => v.Hidden)
                .To(vm => vm.ShowEstimate)
                .WithConversion("BoolInverter");

            set.Bind(_dateTimePicker)
                .For(v => v.DateChangedCommand)
                .To(vm => vm.PickupDateSelectedCommand);

            set.Bind(_bottomAction.ClearLocationButton)
				.For(v => v.Hidden)
                .To(vm => vm.CanClearAddress)
                .WithConversion("BoolInverter");
            set.Bind(_bottomAction.ClearLocationButton)
                .For(v => v.Enabled)
                .To(vm => vm.CanClearAddress);
            set.Bind(_bottomAction.ClearLocationButton)
                .For("TouchUpInside")
                .To(vm => vm.SelectedAddress.ClearPositionCommand);

            set.Bind(_bottomAction.BookNowButton)
                .For("TouchUpInside")
                .To(vm => vm.BookNow);

            set.Bind(_bottomAction.BookLaterButton)
                .For(v => v.Hidden)
                .To(vm => vm.DisableFutureBooking);

            set.Bind(backBtn)
                .For("TouchUpInside")
                .To(vm => vm.ClosePanelCommand);
            set.Bind(backBtn)
                .For(v => v.Hidden)
                .To(vm => vm.Panel.MenuIsOpen)
                .WithConversion("BoolInverter");

			var nib = UINib.FromName ("PanelMenuView", null);
			_menu = (PanelMenuView)nib.Instantiate (this, null)[0];
			_menu.ViewToAnimate = bookView;

			set.Bind (_menu)
				.For (v => v.DataContext)
				.To (vm => vm.Panel);

			set.Apply();


            if (ViewModel.HideDestination)
            {
                var heightToCut = 76f;

                headerBackgroundView.IncrementHeight(-heightToCut);
                mapView.SetY(headerBackgroundView.Frame.Bottom);
                mapView.IncrementHeight(heightToCut);
            }
            
			View.ApplyAppFont ();
            ViewModel.OnViewLoaded();

            View.InsertSubviewBelow (_menu, bookView);
        }

        private bool _firstStart = true;
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            ViewModel.OnViewStarted (_firstStart);
            _firstStart = false;
            NavigationController.NavigationBar.Hidden = true;
        }

        public override void ViewWillDisappear (bool animated)
        {
            base.ViewWillDisappear (animated);
            if(ViewModel!= null) ViewModel.OnViewStopped();
        }
        
        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);

			var button = AppButtons.CreateStandardButton( new RectangleF( 0,2,40,40 ) , "", AppStyle.ButtonColor.Black, "Assets/settings.png");
            button.TouchUpInside += (sender, e) => ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;

            var offsetView = UIButton.FromType(UIButtonType.Custom);
            offsetView.Frame = new RectangleF(0, 0, 60, 44);                
            offsetView.AddSubview ( button );
            offsetView.TouchUpInside += (sender, e) => ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;

            var btn = new UIBarButtonItem ( offsetView );
			navBar.TopItem.LeftBarButtonItem = btn;
            ViewModel.ShowTutorial.Execute ();
        }

        private void OnStatusCloseRequested (StatusCloseRequested msg)
        {
            this.Services().Booking.ClearLastOrder();
            NavigationController.NavigationBar.Hidden = true;
            NavigationController.PopToRootViewController (true);
            ViewModel.Reset ();
            ViewModel.Dropoff.ClearAddress ();        
        }


    }
}

