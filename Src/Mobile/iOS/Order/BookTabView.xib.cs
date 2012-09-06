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
 
namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class BookTabView : UIViewController, ITaxiViewController, ISelectableViewController, IRefreshableViewController
    {
        #region Constructors

        public event EventHandler TabSelected;

        private CreateOrder _bookingInfo;
        private StatusView _statusView;
		private AddressContoller _pickAdrsCtl;
		private AddressContoller _destAdrsCtl;
		private VerticalButtonBar _settingsBar;

        public CreateOrder BookingInfo {
            get { return _bookingInfo; }
            private set { _bookingInfo = value; }
        }

        public BookTabView (IntPtr handle) : base(handle)
        {
            Initialize ();
        }

        [Export("initWithCoder:")]
        public BookTabView (NSCoder coder) : base(coder)
        {
            Initialize ();
        }

        public BookTabView () : base("BookTabView", null)
        {
            Initialize ();
        }

        void Initialize ()
        {
            BookingInfo = new CreateOrder ();
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png")); 

			pickView.Placeholder = Resources.PickupTextPlaceholder;
			destView.Placeholder = Resources.DestinationTextPlaceholder;
			destView.ClearBackground = true;

			AppButtons.FormatStandardGradientButton( (GradientButton)bookBtn, Resources.BookItButton , UIColor.White, AppStyle.ButtonColor.Green );
			bookBtn.TouchUpInside += BookitButtonTouchUpInside;

			AppButtons.FormatStandardGradientButton( (GradientButton)datetimeBtn, Resources.PickupLater , UIColor.White, AppStyle.ButtonColor.Black );
			bookBtn.TouchUpInside += BookitButtonTouchUpInside;

			UIImageView img = new UIImageView(UIImage.FromFile("Assets/location.png"));
			img.BackgroundColor = UIColor.Clear;
			img.Frame = new System.Drawing.RectangleF(mapView.Frame.X + ((mapView.Frame.Width / 2) - 10), mapView.Frame.Y + ((mapView.Frame.Height / 2)) - 30, 20, 20);
			mapView.Superview.AddSubview(img);
			mapView.MultipleTouchEnabled = true;

			_pickAdrsCtl = new AddressContoller( pickView, tableView, mapView, AddressAnnotationType.Pickup, () => BookingInfo.PickupAddress, data => BookingInfo.PickupAddress = data );
			_destAdrsCtl = new AddressContoller( destView, tableView, mapView, AddressAnnotationType.Destination, () => BookingInfo.DropOffAddress, data => BookingInfo.DropOffAddress = data );

			_settingsBar = new VerticalButtonBar( new RectangleF( 9, 6, 40, 33 ), VerticalButtonBar.AnimationType.Wheel, apcurium.MK.Booking.Mobile.Client.VerticalButtonBar.AnimationDirection.Up );
			_settingsBar.ButtonClicked += HandleSettingsButtonClicked;
			bottomBar.AddSubview( _settingsBar );

			View.BringSubviewToFront( destView );
			View.BringSubviewToFront( pickView );
        }

        void HandleSettingsButtonClicked (int index)
        {

        }
        
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);     
            
            if (AppContext.Current.ReceiveMemoryWarning)
            {
                Console.WriteLine (" When view apperaing ReceiveMemoryWarning");
            }
            
            AppContext.Current.ReceiveMemoryWarning = false;              
        }
        
        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            
            if ((AppContext.Current.LastOrder.HasValue) && (AppContext.Current.LoggedUser != null) )
            {
                LoadStatusView (true);
            }
        }

        public string GetTitle ()
        {
            return "";
        }
        
        void BookitButtonTouchUpInside (object sender, EventArgs e)
        {
            BookTaxi ();
        }

        private CreateOrder _toRebookData;

        public void Rebook (Order data)
        {
            //TODO : Fix this
            //_toRebookData = data.Copy ();
        }

        private void RemoveStatusView()
        {
            if (_statusView != null)                   
            {
                try
                {
                    _statusView.View.RemoveFromSuperview();                 
                    _statusView = null;
                }
                catch
                {}
            }
        }

        private void LoadStatusView (Order order, OrderStatusDetail status, bool closeScreenWhenCompleted)
        {
            InvokeOnMainThread (() => {
	            RemoveStatusView();
	            _statusView = new StatusView (this, order, status, closeScreenWhenCompleted);
				_statusView.HidesBottomBarWhenPushed = true;
	            _statusView.CloseRequested += delegate(object sender, EventArgs e) {
		            RemoveStatusView();
		            AppContext.Current.LastOrder = null;
					NavigationController.TabBarController.TabBar.Hidden = false;
		            Selected ();
				};

				NavigationController.PushViewController( _statusView, true );
	        }); 
        }

        void StatusViewCloseRequested (object sender, EventArgs e)
		{}

        private void LoadStatusView ( bool closeScreenWhenCompleted)
        {
            if ( AppContext.Current.LastOrder.HasValue )
            {
                try
                {
                    var order = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrder( AppContext.Current.LastOrder.Value );
                    var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus ( AppContext.Current.LastOrder.Value );
                    LoadStatusView (order, status, closeScreenWhenCompleted);
                }
                catch(Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError( ex );
                    AppContext.Current.LastOrder = null;
                }
            }
        }

        public void Selected ()
        {
            try
            {
                AppContext.Current.ResetPosition ();

                if (AppContext.Current.LastOrder.HasValue) 
                {
                    LoadStatusView (true);
                }
                else
                {  
                    if (!(this.NavigationController.TopViewController is BookTabView))
                    {
                        this.NavigationController.PopViewControllerAnimated (false);
                    }
                    
                    bool isRebook = false;
                    if (_toRebookData != null)
                    {                       
                        BookingInfo = _toRebookData;
                        isRebook = true;
                        _toRebookData = null;    
                    }
                    else
                    {
                        BookingInfo = new CreateOrder ();
                        BookingInfo.Settings = AppContext.Current.LoggedUser.Settings.Copy ();
                    }

					_pickAdrsCtl.Maybe (() => _pickAdrsCtl.AssignData ());
					_destAdrsCtl.Maybe (() => _destAdrsCtl.AssignData ());
                    
                    if ((!isRebook) && (TabSelected != null))
                    {
                        TabSelected (this, EventArgs.Empty);
                    }
                }
            }
            catch
            {
            }
        }

        public void RefreshData ()
        {
            Selected ();
			_pickAdrsCtl.Maybe (() => _pickAdrsCtl.RefreshData ());
			_destAdrsCtl.Maybe (() => _destAdrsCtl.RefreshData ());
        }

        public void BookTaxi ()
        {
            if (BookingInfo == null)
            {
                return;
            }

            if (BookingInfo.Settings.Passengers == 0)
            {
                BookingInfo.Settings = AppContext.Current.LoggedUser.Settings;
            }

            LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
            View.UserInteractionEnabled = false;
            
            ThreadHelper.ExecuteInThread (() =>
            {
                try
                {
					_pickAdrsCtl.SuspendRegionChanged ();
                    _destAdrsCtl.SuspendRegionChanged ();
					_pickAdrsCtl.PrepareData ();
                    _destAdrsCtl.PrepareData ();
                    
                    bool isValid = TinyIoCContainer.Current.Resolve<IBookingService> ().IsValid (ref _bookingInfo);
                    if (!isValid)
                    {
                        MessageHelper.Show (Resources.InvalidBookinInfoTitle, Resources.InvalidBookinInfo);
                        return;
                    }
                    
                    if (_bookingInfo.PickupDate.HasValue && _bookingInfo.PickupDate.Value < DateTime.Now)
                    {
                        MessageHelper.Show (Resources.InvalidBookinInfoTitle, Resources.BookViewInvalidDate);
                        return;
                    }

                    this.InvokeOnMainThread (() =>
                    {
                        var view = new ConfirmationView (this);
						view.HidesBottomBarWhenPushed = true;
                        this.NavigationController.PushViewController (view, true);
                        
                        view.Canceled += delegate(object sender, EventArgs e) {
                            this.NavigationController.PopViewControllerAnimated (true); };
                        
                        view.Confirmed += delegate(object sender, EventArgs e) {
                            CreateOrder (view.BI); };

                        view.NoteChanged += delegate(string note) {
                            BookingInfo.Note = note;
                        };
                    });
                }
                finally
                {
                    InvokeOnMainThread (() =>
                    {
                        _pickAdrsCtl.ResumeRegionChanged ();
                        _destAdrsCtl.ResumeRegionChanged ();
                        LoadingOverlay.StopAnimatingLoading (this.View);
                        View.UserInteractionEnabled = true;
                    });
                }
                
            });
        }

        public bool IsTopView {

            get { return this.NavigationController.TopViewController is BookTabView; }
        }

		public UIView GetTopView()
		{
			return null;
		}

        private void CreateOrder (CreateOrder bi)
        {
            if (!(this.NavigationController.TopViewController is BookTabView))
            {
                this.NavigationController.PopViewControllerAnimated (false);
            }
            
            LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
            View.UserInteractionEnabled = false;
            
            ThreadHelper.ExecuteInThread (() =>
            {
                try
                {
                    var service = TinyIoCContainer.Current.Resolve<IBookingService> ();
                    
                    string error;
                    
                    if (!AppSettings.ShowNumberOfTaxi)
                    {
                        bi.Settings.NumberOfTaxi = 1;
                    }
                

                    bi.Id = Guid.NewGuid();

                    var orderStatus = service.CreateOrder( bi );
                    orderStatus.OrderId = bi.Id;

                    if (orderStatus.IBSOrderId.HasValue)
                    {
                        AppContext.Current.LastOrder = orderStatus.OrderId;

                        //var order = TinyIoCContainer.Current.Resolve<IAccountService>( ).GetHistoryOrder( orderStatus.OrderId );

                        var accountId = TinyIoCContainer.Current.Resolve<IAccountService>( ).CurrentAccount.Id;

                        BookingInfo.Id = orderStatus.OrderId;
                        
                        BookingInfo.PickupDate = DateTime.Now;                       
                        
                        LoadStatusView (new Order { Id = bi.Id, IBSOrderId = orderStatus.IBSOrderId,  CreatedDate = DateTime.Now, DropOffAddress = bi.DropOffAddress, PickupAddress  = bi.PickupAddress , Settings = bi.Settings   }, orderStatus, false);
                    }
                    else
                    {
                        if (error.HasValue ())
                        {
                            MessageHelper.Show (Resources.ErrorCreatingOrderTitle, Resources.ErrorCreatingOrderMessage);
                        }
                        else
                        {
                            MessageHelper.Show (Resources.ErrorCreatingOrderTitle, Resources.ErrorCreatingOrderMessage);
                        }
                    }   
                }
                finally
                {
                    InvokeOnMainThread (() =>
                    {
                        LoadingOverlay.StopAnimatingLoading (this.View);
                        View.UserInteractionEnabled = true;
                    });
                }
            });
            
        }

        #endregion
    }
}

