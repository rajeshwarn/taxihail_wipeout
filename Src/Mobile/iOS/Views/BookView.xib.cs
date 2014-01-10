using System;
using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class BookView : MvxViewController , INavigationView
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

        #region INavigationView implementation

        public bool HideNavigationBar {
            get { return true;}
        }

        #endregion

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            navBar.SetBackgroundImage (UIImage.FromFile ("Assets/navBar.png"), UIBarMetrics.Default);
            navBar.TopItem.TitleView = new TitleView (null, "", false);

            bookView.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
      
                               
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub> ().Subscribe<StatusCloseRequested> (OnStatusCloseRequested);
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub> ().Subscribe<DateTimePicked> (msg => _onDateTimePicked ());
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

            this.AddBindings (new Dictionary<object, string> ()                            {

                { _bottomAction.RefreshCurrentLocationButton, "{'TouchUpInside':{'Path':'SelectedAddress.RequestCurrentLocationCommand'}}"},                
                { pickupActivationButton, "{'TouchUpInside':{'Path':'ActivatePickup'},'Selected':{'Path':'AddressSelectionMode', 'Converter': 'EnumToBool', 'ConverterParameter': 'PickupSelection'}}"},                
                { dropoffActivationButton, "{'TouchUpInside':{'Path':'ActivateDropoff'},'Selected':{'Path':'AddressSelectionMode', 'Converter': 'EnumToBool', 'ConverterParameter': 'DropoffSelection'}, 'Hidden': {'Path': 'HideDestination'}}"},       
                { pickupButton, "{'TouchUpInside':{'Path':'Pickup.PickAddress'},'TextLine1':{'Path':'Pickup.AddressLine1', 'Mode':'TwoWay'}, 'TextLine2':{'Path':'Pickup.AddressLine2', 'Mode':'TwoWay'}, 'IsSearching':{'Path':'Pickup.IsExecuting', 'Mode':'TwoWay'}, 'IsPlaceholder':{'Path':'Pickup.IsPlaceHolder', 'Mode':'TwoWay'} }"},  
                { dropoffButton, "{'TouchUpInside':{'Path':'Dropoff.PickAddress'},'TextLine1':{'Path':'Dropoff.AddressLine1', 'Mode':'TwoWay'}, 'TextLine2':{'Path':'Dropoff.AddressLine2', 'Mode':'TwoWay'}, 'IsSearching':{'Path':'Dropoff.IsExecuting', 'Mode':'TwoWay'}, 'IsPlaceholder':{'Path':'Dropoff.IsPlaceHolder', 'Mode':'TwoWay'}, 'Hidden': {'Path': 'HideDestination'}}"},             
                { mapView, @"{
'Pickup':{'Path':'Pickup.Model'},
'Dropoff':{'Path':'Dropoff.Model'},
'MapMoved':{'Path':'SelectedAddress.SearchCommand'},
'MapCenter':{'Path':'MapCenter'},
'AvailableVehicles': {'Path': 'AvailableVehicles'},
'AddressSelectionMode': {'Path': 'AddressSelectionMode'}
}" },
                { infoLabel, "{'Text':{'Path':'FareEstimate'}, 'Hidden':{'Path':'ShowEstimate', 'Converter':'BoolInverter'}}" },              
                { _dateTimePicker, "{'DateChangedCommand':{'Path':'PickupDateSelectedCommand'}}" },
                { _bottomAction.ClearLocationButton, "{'Hidden':{'Path':'CanClearAddress', 'Converter':'BoolInverter'}, 'Enabled':{'Path':'CanClearAddress'}, 'TouchUpInside':{'Path':'SelectedAddress.ClearPositionCommand'}}" },
                { _bottomAction.BookNowButton , "{'TouchUpInside': {'Path': 'BookNow'}}" },
                { _bottomAction.BookLaterButton , "{'Hidden': {'Path': 'DisableFutureBooking'}}" },

                { backBtn , "{'TouchUpInside': {'Path': 'ClosePanelCommand'},'Hidden': {'Path': 'Panel.MenuIsOpen', 'Converter': 'BoolInverter'}}" }        
            });

            if (ViewModel.HideDestination)
            {
                var heightToCut = 76f;

                headerBackgroundView.IncrementHeight(-heightToCut);
                mapView.SetY(headerBackgroundView.Frame.Bottom);
                mapView.IncrementHeight(heightToCut);
            }
             
            View.ApplyAppFont ();
            ViewModel.Load();

            _menu = new PanelMenuView (bookView, ViewModel.Panel);
            View.InsertSubviewBelow (_menu.View, bookView);
        }

        private bool _firstStart = true;
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            if (_firstStart) {
                _firstStart = false;
                ViewModel.Start (firstStart: true);
            } else {
                ViewModel.Restart();
                ViewModel.Start(firstStart: false);
            }
            NavigationController.NavigationBar.Hidden = true;

        }

        public override void ViewWillDisappear (bool animated)
        {
            base.ViewWillDisappear (animated);
            if(ViewModel!= null) ViewModel.Stop();
        }
        
        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);

            var button = AppButtons.CreateStandardButton( new RectangleF( 16,2,40,40 ) , "", AppStyle.ButtonColor.Black, "Assets/settings.png");
            button.TouchUpInside += (sender, e) => ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;

            var offsetView = UIButton.FromType(UIButtonType.Custom);
            offsetView.Frame = new RectangleF(0, 0, 60, 44);                
            offsetView.AddSubview ( button );
            offsetView.TouchUpInside += (sender, e) => ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;

            var btn = new UIBarButtonItem ( offsetView );
            navBar.TopItem.RightBarButtonItem = btn;
            ViewModel.ShowTutorial.Execute ();
        }

        private void OnStatusCloseRequested (StatusCloseRequested msg)
        {
			TinyIoCContainer.Current.Resolve<IBookingService> ().ClearLastOrder();
            NavigationController.NavigationBar.Hidden = true;
            NavigationController.PopToRootViewController (true);
            ViewModel.Reset ();
            ViewModel.Dropoff.ClearAddress ();        
        }



    }
}

