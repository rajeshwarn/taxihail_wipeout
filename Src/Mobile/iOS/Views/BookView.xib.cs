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
 
namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class BookView : MvxBindingTouchViewController<BookViewModel> , INavigationView
    {
        #region Constructors

		private PanelMenuView _menu;
		private DateTimePicker _dateTimePicker;
		private Action _onDateTimePicked;
        private StatusView _statusView;
		private UIImageView _img;

        public BookView() 
            : base(new MvxShowViewModelRequest<BookViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }

		public BookView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
		public BookView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }

        #region INavigationView implementation

        public bool HideNavigationBar
        {
            get { return true;}
        }

        #endregion

        public CreateOrder BookingInfo
        {
            get { return ViewModel.Order; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			navBar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
			navBar.TopItem.TitleView = new TitleView( null, "", false );

			bookView.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
			_menu = new PanelMenuView( bookView, this.NavigationController );
			View.InsertSubviewBelow( _menu.View, bookView );

            AppButtons.FormatStandardButton((GradientButton)refreshCurrentLocationButton, "", AppStyle.ButtonColor.Blue, "");
			AppButtons.FormatStandardButton((GradientButton)cancelBtn, "", AppStyle.ButtonColor.Red, "Assets/cancel.png");

			TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub>().Subscribe<StatusCloseRequested>(OnStatusCloseRequested);

			TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub>().Subscribe<RebookRequested>( msg => {
				ViewModel.Rebook( msg.Content );
			});
			TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub>().Subscribe<DateTimePicked>( msg => _onDateTimePicked() );
			_dateTimePicker = new DateTimePicker( );
			_dateTimePicker.ShowPastDate = false;
			_onDateTimePicked = () => _dateTimePicker.Hide();

			View.AddSubview( _dateTimePicker );

			AppButtons.FormatStandardButton((GradientButton)bookLaterButton, "", AppStyle.ButtonColor.DarkGray );

			bookLaterButton.TouchUpInside += delegate {
				_dateTimePicker.Show( ViewModel.Order.PickupDate );
			};


            AppButtons.FormatStandardButton((GradientButton)dropoffButton, "", AppStyle.ButtonColor.Grey, "");
            AppButtons.FormatStandardButton((GradientButton)pickupButton, "", AppStyle.ButtonColor.Grey );

            AppButtons.FormatStandardButton((GradientButton)dropoffActivationButton, "", AppStyle.ButtonColor.LightBlue, "");
            AppButtons.FormatStandardButton((GradientButton)pickupActivationButton, "", AppStyle.ButtonColor.LightBlue  );

            ((GradientButton)dropoffActivationButton).SetImage( "Assets/pin.png" );
            ((GradientButton)pickupActivationButton).SetImage( "Assets/pin.png" );

            ((GradientButton)dropoffActivationButton).SetSelectedImage( "Assets/pin_selected.png" );
            ((GradientButton)pickupActivationButton).SetSelectedImage( "Assets/pin_selected.png" );

            headerBackgroundView.Image =UIImage.FromFile("Assets/backPickupDestination.png");

            ((GradientButton)bookLaterButton).SetImage( "Assets/bookLaterIcon.png" );
            ((GradientButton)refreshCurrentLocationButton).SetImage( "Assets/gpsRefreshIcon.png" );

            AppButtons.FormatStandardButton((GradientButton)bookBtn, Resources.BookItButton, AppStyle.ButtonColor.Green);
            bookBtn.TouchUpInside -= BookitButtonTouchUpInside;
            bookBtn.TouchUpInside += BookitButtonTouchUpInside;


            mapView.MultipleTouchEnabled = true;
			mapView.Delegate = new AddressMapDelegate();

            bottomBar.UserInteractionEnabled = true;
            bookView.BringSubviewToFront(bottomBar);
            bookView.BringSubviewToFront(bookBtn);

            this.AddBindings(new Dictionary<object, string>()                            {
                { refreshCurrentLocationButton, "{'TouchUpInside':{'Path':'SelectedAddress.RequestCurrentLocationCommand'}}"},                
                { pickupActivationButton, "{'TouchUpInside':{'Path':'ActivatePickup'},'Selected':{'Path':'PickupIsActive', 'Mode':'TwoWay'}}"},                
                { dropoffActivationButton, "{'TouchUpInside':{'Path':'ActivateDropoff'},'Selected':{'Path':'DropoffIsActive', 'Mode':'TwoWay'}}"},       
				{ pickupButton, "{'TouchUpInside':{'Path':'Pickup.PickAddress'},'TextLine1':{'Path':'Pickup.Title', 'Mode':'TwoWay'}, 'TextLine2':{'Path':'Pickup.Display', 'Mode':'TwoWay'}, 'IsSearching':{'Path':'Pickup.IsExecuting', 'Mode':'TwoWay'}, 'IsPlaceholder':{'Path':'Pickup.IsPlaceHolder', 'Mode':'TwoWay'} }"},  
				{ dropoffButton, "{'TouchUpInside':{'Path':'Dropoff.PickAddress'},'TextLine1':{'Path':'Dropoff.Title', 'Mode':'TwoWay'}, 'TextLine2':{'Path':'Dropoff.Display', 'Mode':'TwoWay'}, 'IsSearching':{'Path':'Dropoff.IsExecuting', 'Mode':'TwoWay'}, 'IsPlaceholder':{'Path':'Dropoff.IsPlaceHolder', 'Mode':'TwoWay'} }"},             
				{ mapView, "{'Pickup':{'Path':'Pickup.Model'}, 'Dropoff':{'Path':'Dropoff.Model'} , 'MapMoved':{'Path':'SelectedAddress.SearchCommand'}, 'MapCenter':{'Path':'MapCenter'} }" },
				{ infoLabel, "{'Text':{'Path':'FareEstimate'}}" },
				{ pickupDateLabel, "{'Text':{'Path':'PickupDateDisplay'}, 'Hidden':{'Path':'IsInTheFuture','Converter':'BoolInverter'}}" },
				{ _dateTimePicker, "{'DateChangedCommand':{'Path':'PickupDateSelectedCommand'}, 'CloseDatePickerCommand':{'Path':'CloseDatePickerCommand'}}" },
				{ cancelBtn, "{'Hidden':{'Path':'CanClearAddress', 'Converter':'BoolInverter'}, 'Enabled':{'Path':'CanClearAddress'}, 'TouchUpInside':{'Path':'SelectedAddress.ClearPositionCommand'}}" },
		
            });

			if (ViewModel != null)
			{
				ViewModel.Initialize();
			}

        }

        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);     

			NavigationController.NavigationBar.Hidden = true;
            AppContext.Current.ReceiveMemoryWarning = false;  

			if( _img == null )
			{
				_img = new UIImageView(UIImage.FromFile("Assets/location.png"));
				_img.BackgroundColor = UIColor.Clear;
				_img.ContentMode = UIViewContentMode.Center;
				_img.Frame = new System.Drawing.RectangleF(mapView.Frame.X + ((mapView.Frame.Width / 2) - 10), mapView.Frame.Y + (mapView.Frame.Height / 2), 20, 20);
				mapView.Superview.AddSubview(_img);
			}
        }
        
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

			var btn = new UIBarButtonItem( new BarButtonItem( new RectangleF(0,0, 40, 33) , "Assets/settings.png", () => _menu.AnimateMenu() ) );
			navBar.TopItem.RightBarButtonItem = btn;
			navBar.TopItem.RightBarButtonItem.SetTitlePositionAdjustment( new UIOffset( -10,0 ), UIBarMetrics.Default );
            var account = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;
            if ((AppContext.Current.LastOrder.HasValue) && (account != null))
            {
                LoadStatusView(true);
            }
        }

        void BookitButtonTouchUpInside(object sender, EventArgs e)
        {
            BookTaxi();
        }


        /*private void RemoveStatusView()
        {
            if (_statusView != null)
            {
                try
                {
                    _statusView.View.RemoveFromSuperview();                 
                    _statusView = null;
                }
                catch
                {
                }
            }
        }*/

        private void LoadStatusView(Order order, OrderStatusDetail status, bool closeScreenWhenCompleted)
        {
            InvokeOnMainThread(() => {
                //RemoveStatusView();
				var param = new Dictionary<string, object>() {{"order", order}, {"orderInfo", status}};
				ViewModel.NavigateToOrderStatus.Execute(param);


                //_statusView = new StatusView(this, order, status, closeScreenWhenCompleted);
                //_statusView.HidesBottomBarWhenPushed = true;
                /*_statusView.CloseRequested += delegate(object sender, EventArgs e)
                {
                    RemoveStatusView();
                    

                };*/

                //NavigationController.PushViewController(_statusView, true);
            }); 
        }

        private void LoadStatusView(bool closeScreenWhenCompleted)
        {
            if (AppContext.Current.LastOrder.HasValue)
            {
                try
                {
                    var order = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrder(AppContext.Current.LastOrder.Value);
                    var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(AppContext.Current.LastOrder.Value);
                    LoadStatusView(order, status, closeScreenWhenCompleted);
                }
                catch (Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                    AppContext.Current.LastOrder = null;
                }
            }
        }

		private void OnStatusCloseRequested(StatusCloseRequested msg)
		{
			AppContext.Current.LastOrder = null;
			NavigationController.NavigationBar.Hidden = true;
			this.NavigationController.PopToRootViewController(true);
			ViewModel.Reset();
			ViewModel.Dropoff.ClearAddress();
			ViewModel.Initialize();
		}

        public void BookTaxi()
        {
            if (BookingInfo == null)
            {
                return;
            }

            if (BookingInfo.Settings.Passengers == 0)
            {
                var account = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;
                BookingInfo.Settings = account.Settings;
            }

            LoadingOverlay.StartAnimatingLoading(LoadingOverlayPosition.Center, null, 130, 30);
            bookView.UserInteractionEnabled = false;
            
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    var bookingInfo = BookingInfo;
                    bool isValid = TinyIoCContainer.Current.Resolve<IBookingService>().IsValid(ref bookingInfo);
                    if (!isValid)
                    {
                        MessageHelper.Show(Resources.InvalidBookinInfoTitle, Resources.InvalidBookinInfo);
                        return;
                    }
                    
                    if (BookingInfo.PickupDate.HasValue && BookingInfo.PickupDate.Value < DateTime.Now)
                    {
                        MessageHelper.Show(Resources.InvalidBookinInfoTitle, Resources.BookViewInvalidDate);
                        return;
                    }

                    this.InvokeOnMainThread(() =>
                    {
                        var view = new ConfirmationView(ViewModel.Order);
                        view.HidesBottomBarWhenPushed = true;
                        this.NavigationController.PushViewController(view, true);
                        
                        view.Canceled += delegate(object sender, EventArgs e)
                        {
                            this.NavigationController.PopViewControllerAnimated(true);

						};
                        
                        view.Confirmed += delegate(object sender, EventArgs e)
                        {
                            CreateOrder(view.Order);
                        };



                    });
                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        LoadingOverlay.StopAnimatingLoading();
                        bookView.UserInteractionEnabled = true;
                    });
                }
                
            });
        }




        private void CreateOrder(CreateOrder bi)
        {
            if (!(this.NavigationController.TopViewController is BookView))
            {
                this.NavigationController.PopViewControllerAnimated(false);
            }
            
            LoadingOverlay.StartAnimatingLoading( LoadingOverlayPosition.Center, null, 130, 30);
            bookView.UserInteractionEnabled = false;
            
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    var service = TinyIoCContainer.Current.Resolve<IBookingService>();                   

                    bi.Settings.NumberOfTaxi = 1;                

                    bi.Id = Guid.NewGuid();

                    var orderStatus = service.CreateOrder(bi);
                    if ( orderStatus != null )
                    {
                    orderStatus.OrderId = bi.Id;

                    if ( orderStatus.IBSOrderId.HasValue)
                    {


                        AppContext.Current.LastOrder = orderStatus.OrderId;

                        var accountId = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount.Id;

                        BookingInfo.Id = orderStatus.OrderId;
                        
                        BookingInfo.PickupDate = DateTime.Now;                       

						LoadStatusView(new Order { Id = bi.Id, IBSOrderId = orderStatus.IBSOrderId,  CreatedDate = DateTime.Now, DropOffAddress = bi.DropOffAddress, PickupAddress  = bi.PickupAddress , Settings = bi.Settings   }, orderStatus, false);

                    }
                    }
                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        LoadingOverlay.StopAnimatingLoading();
                        bookView.UserInteractionEnabled = true;
                    });
                }
            });
            
        }
	



        #endregion



    }
}

