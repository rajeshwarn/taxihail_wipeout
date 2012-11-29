using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.Text;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Interfaces;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using apcurium.MK.Booking.Mobile.ListViewStructure;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Animations;
using MonoTouch.MessageUI;
using System.IO;
using apcurium.MK.Booking.Mobile.Client.MapUtilities;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Messages;
using System.Threading;
 
namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class BookView : MvxBindingTouchViewController<BookViewModel> , INavigationView
    {
        #region Constructors

        private PanelMenuView _menu;
        private DateTimePicker _dateTimePicker;
        private Action _onDateTimePicked;
        private BookViewActionsView _bottomAction;

        public BookView () 
            : base(new MvxShowViewModelRequest<BookViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }

        public BookView (MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public BookView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
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
            _menu = new PanelMenuView (bookView, this.NavigationController, ViewModel.Panel);
            View.InsertSubviewBelow (_menu.View, bookView);

            AppButtons.FormatStandardButton ((GradientButton)refreshCurrentLocationButton, "", AppStyle.ButtonColor.Blue, "");
            AppButtons.FormatStandardButton ((GradientButton)cancelBtn, "", AppStyle.ButtonColor.Red, "Assets/cancel.png");

            TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub> ().Subscribe<StatusCloseRequested> (OnStatusCloseRequested);

            TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub> ().Subscribe<DateTimePicked> (msg => _onDateTimePicked ());
            _dateTimePicker = new DateTimePicker ();
            _dateTimePicker.ShowPastDate = false;
            _onDateTimePicked = () => _dateTimePicker.Hide ();

            View.AddSubview (_dateTimePicker);

            AppButtons.FormatStandardButton ((GradientButton)bookLaterButton, "", AppStyle.ButtonColor.DarkGray);


            bookLaterButton.TouchUpInside += delegate {
                _dateTimePicker.Show (ViewModel.Order.PickupDate);
            };                      
           
            AppButtons.FormatStandardButton ((GradientButton)dropoffActivationButton, "", AppStyle.ButtonColor.LightBlue, "");
            AppButtons.FormatStandardButton ((GradientButton)pickupActivationButton, "", AppStyle.ButtonColor.LightBlue);

            ((GradientButton)dropoffActivationButton).SetImage ("Assets/flag.png");
            ((GradientButton)pickupActivationButton).SetImage ("Assets/hail.png");

            ((GradientButton)dropoffActivationButton).SetSelectedImage ("Assets/flag_selected.png");
            ((GradientButton)pickupActivationButton).SetSelectedImage ("Assets/hail_selected.png");

            headerBackgroundView.Image = UIImage.FromFile ("Assets/backPickupDestination.png");

            ((GradientButton)bookLaterButton).SetImage ("Assets/bookLaterIcon.png");
            ((GradientButton)refreshCurrentLocationButton).SetImage ("Assets/gpsRefreshIcon.png");

            AppButtons.FormatStandardButton ((GradientButton)bookBtn, Resources.BookItButton, AppStyle.ButtonColor.Green);

            mapView.MultipleTouchEnabled = true;
            mapView.Delegate = new AddressMapDelegate ();

            bottomBar.UserInteractionEnabled = true;
            bookView.BringSubviewToFront (bottomBar);
            bookView.BringSubviewToFront (bookBtn);

            _bottomAction = new BookViewActionsView();
            bottomBar.Subviews.ForEach ( v=>v.Hidden = true );
            _bottomAction.Frame = new RectangleF( 0,0, bottomBar.Bounds.Width, bottomBar.Bounds.Height );
            bottomBar.AddSubview( _bottomAction );

            _bottomAction.BookLaterButton.TouchUpInside += delegate {
                                _dateTimePicker.Show (ViewModel.Order.PickupDate);
                            };                      


            this.AddBindings (new Dictionary<object, string> ()                            {
                { _bottomAction.RefreshCurrentLocationButton, "{'TouchUpInside':{'Path':'SelectedAddress.RequestCurrentLocationCommand'}}"},                
                { pickupActivationButton, "{'TouchUpInside':{'Path':'ActivatePickup'},'Selected':{'Path':'PickupIsActive', 'Mode':'TwoWay'}}"},                
                { dropoffActivationButton, "{'TouchUpInside':{'Path':'ActivateDropoff'},'Selected':{'Path':'DropoffIsActive', 'Mode':'TwoWay'}}"},       
                { pickupButton, "{'TouchUpInside':{'Path':'Pickup.PickAddress'},'TextLine1':{'Path':'Pickup.Title', 'Mode':'TwoWay'}, 'TextLine2':{'Path':'Pickup.Display', 'Mode':'TwoWay'}, 'IsSearching':{'Path':'Pickup.IsExecuting', 'Mode':'TwoWay'}, 'IsPlaceholder':{'Path':'Pickup.IsPlaceHolder', 'Mode':'TwoWay'} }"},  
                { dropoffButton, "{'TouchUpInside':{'Path':'Dropoff.PickAddress'},'TextLine1':{'Path':'Dropoff.Title', 'Mode':'TwoWay'}, 'TextLine2':{'Path':'Dropoff.Display', 'Mode':'TwoWay'}, 'IsSearching':{'Path':'Dropoff.IsExecuting', 'Mode':'TwoWay'}, 'IsPlaceholder':{'Path':'Dropoff.IsPlaceHolder', 'Mode':'TwoWay'} }"},             
                { mapView, "{'Pickup':{'Path':'Pickup.Model'}, 'Dropoff':{'Path':'Dropoff.Model'} , 'MapMoved':{'Path':'SelectedAddress.SearchCommand'}, 'MapCenter':{'Path':'MapCenter'}, 'PickupIsActive': {'Path': 'PickupIsActive'}, 'DropoffIsActive': {'Path': 'DropoffIsActive'} }" },
                { infoLabel, "{'Text':{'Path':'FareEstimate'}}" },
                { pickupDateLabel, "{'Text':{'Path':'PickupDateDisplay'}, 'Hidden':{'Path':'IsInTheFuture','Converter':'BoolInverter'}}" },
                { _dateTimePicker, "{'DateChangedCommand':{'Path':'PickupDateSelectedCommand'}, 'CloseDatePickerCommand':{'Path':'CloseDatePickerCommand'}}" },
                { _bottomAction.ClearLocationButton, "{'Hidden':{'Path':'CanClearAddress', 'Converter':'BoolInverter'}, 'Enabled':{'Path':'CanClearAddress'}, 'TouchUpInside':{'Path':'SelectedAddress.ClearPositionCommand'}}" },
                { _bottomAction.BookNowButton , "{'TouchUpInside': {'Path': 'BookTaxi'}}" }                  
            });

            this.View.ApplyAppFont ();
        }

        protected override void OnViewModelChanged ()
        {
            base.OnViewModelChanged ();
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);     

            NavigationController.NavigationBar.Hidden = true;
            AppContext.Current.ReceiveMemoryWarning = false;  

        }
        
        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);

            var btn = new UIBarButtonItem (new BarButtonItem (new RectangleF (0, 0, 40, 33), "Assets/settings.png", () => ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen));
            navBar.TopItem.RightBarButtonItem = btn;
            navBar.TopItem.RightBarButtonItem.SetTitlePositionAdjustment (new UIOffset (-10, 0), UIBarMetrics.Default);
        }

        private void OnStatusCloseRequested (StatusCloseRequested msg)
        {
			TinyIoCContainer.Current.Resolve<IBookingService> ().ClearLastOrder();
            NavigationController.NavigationBar.Hidden = true;
            this.NavigationController.PopToRootViewController (true);
            ViewModel.Reset ();
            ViewModel.Dropoff.ClearAddress ();
            //ViewModel.Initialize ();
        }

        #endregion



    }
}

