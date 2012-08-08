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

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code

        public event EventHandler TabSelected;

        private CreateOrder _bookingInfo;
        private PickupLocationView _pickup;
        private DestinationView _destination;
        private UIButton _btnBookIt;
        private StatusView _statusView;
        private DestPickToggleButton _destPickToggleButton;

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
            scrollView.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
        

            AppContext.Current.Controller.CompanyChanged -= ChangeCompany;
            AppContext.Current.Controller.CompanyChanged += ChangeCompany;

            
        }
        
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);     
            
            
            if ((_pickup == null) || (_pickup.View == null) || (_pickup.View.Superview == null))
            {           
                CreatePanels ();
                _destPickToggleButton = new DestPickToggleButton (viewPick, viewDest, () => SwitchToPickView (), () => SwitchToDestView ());                        
            }
            
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
                scrollView.Hidden = true;
                LoadStatusView (true);
            }
        }
        
        private void CreatePanels ()
        {
            scrollView.Scrolled -= ScrollViewScrolled;
            scrollView.Scrolled += ScrollViewScrolled;
            
            int count = 2;
            RectangleF scrollFrame = scrollView.Frame;
            scrollFrame.Width = scrollFrame.Width * count;
            scrollView.ContentSize = scrollFrame.Size;
            
            _pickup = new PickupLocationView (this);                        
            _destination = new DestinationView (this);
            
            
            RectangleF frame = scrollView.Frame;
            
            PointF location = new PointF ();
            location.X = frame.Width * 0;
            frame.Location = location;
            _pickup.View.Frame = frame;
            scrollView.Add (_pickup.View);
            
            
            location = new PointF ();
            location.X = frame.Width * 1;
            frame.Location = location;
            _destination.View.Frame = frame;
            scrollView.Add (_destination.View);
            
            
            pageControl.ValueChanged -= PageChanged;
            pageControl.ValueChanged += PageChanged;
            pageControl.Pages = count;

            _destination.StartTrackingMapMoving();
            _pickup.StartTrackingMapMoving();
        }

        private void SwitchToPickView ()
        {
            SwitchToPage (0);
        }

        private void SwitchToDestView ()
        {
            SwitchToPage (1);
        }

        private void SwitchToPage (int toPage)
        {
            int fromPage = (int)Math.Floor ((scrollView.ContentOffset.X - scrollView.Frame.Width / 2) / scrollView.Frame.Width) + 1;
            
            var pageOffset = scrollView.Frame.Width * toPage;
            PointF p = new PointF (pageOffset, 0);
            pageControl.CurrentPage = fromPage;
            scrollView.SetContentOffset (p, true);
        }

        public UIView GetTopView ()
        {
            _btnBookIt =AppButtons.CreateStandardGradientButton( new System.Drawing.RectangleF (210, 6, 98, 37), "Book It!", UIColor.White, AppStyle.ButtonColor.Green )  ;
            ((GradientButton) _btnBookIt).TitleFont = AppStyle.GetButtonFont( 16 );
            _btnBookIt.TouchUpInside += BookitButtonTouchUpInside;            
            return _btnBookIt;
            
        }

        public string GetTitle ()
        {
            return "";
        }
        
        void ChangeCompany (object sender, EventArgs e)
        {
        
            if (_bookingInfo != null)
            {
                var id = (AppContext.Current.LoggedUser != null) && (AppContext.Current.LoggedUser.Settings != null) ? AppContext.Current.LoggedUser.Settings.ProviderId : -1;
                 
                _bookingInfo.Settings.ProviderId = id;              
            }
            
                
        }

        void BookitButtonTouchUpInside (object sender, EventArgs e)
        {
            BookTaxi ();
        }

        void PageChanged (object sender, EventArgs e)
        {
            var pc = (UIPageControl)sender;
            int fromPage = (int)Math.Floor ((scrollView.ContentOffset.X - scrollView.Frame.Width / 2) / scrollView.Frame.Width) + 1;
            var toPage = pc.CurrentPage;
            var pageOffset = scrollView.Frame.Width * toPage;
            PointF p = new PointF (pageOffset, 0);
            pc.CurrentPage = fromPage;
            scrollView.SetContentOffset (p, true);
        }

        private void ScrollViewScrolled (object sender, EventArgs e)
        {
            double page = Math.Floor ((scrollView.ContentOffset.X - scrollView.Frame.Width / 2) / scrollView.Frame.Width) + 1;
            pageControl.CurrentPage = (int)page;
            _destPickToggleButton.SelectedButton = (int)page;
            
            if ((int)page == 0)
            {
                _pickup.Display ();
                
            }
            else
            if ((int)page == 1)
            {
                _destination.Display ();
            }
        }

        private CreateOrder _toRebookData;

        public void Rebook (Order data)
        {
            pageControl.CurrentPage = 0;

            //TODO : Fix this
            //_toRebookData = data.Copy ();
        }

//      private bool TryToLoadLastOrder ()
//      {
//          bool wasReloaded = false;
//          
//          if (AppContext.Current.LastOrder.HasValue && !TinyIoCContainer.Current.Resolve<IBookingService> ().IsCompleted (AppContext.Current.LoggedUser, AppContext.Current.LastOrder.Value))
//          {
//              
//              var order = AppContext.Current.LoggedUser.BookingHistory.FirstOrDefault (o => o.Id == AppContext.Current.LastOrder.Value);
//              if (order != null)
//              {
//                  
//                  wasReloaded = true;
//                  StatusView view = null;
//                  
//                  if ((this.NavigationController.TopViewController is StatusView))
//                  {
//                      view = (StatusView)this.NavigationController.TopViewController;
//                      view.Refresh (order);
//                  }
//
//                  else
//                  {
//                      if (!(this.NavigationController.TopViewController is BookTabView))
//                      {
//                          this.NavigationController.PopViewControllerAnimated (false);
//                      }
//                      view = new StatusView (this, order);
//                      this.NavigationController.PushViewController (view, true);
//                  }
//                  
//              }
//              
//              
//              
//              
//              
//          }
//          
//          return wasReloaded;
//      }


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
                {
                }
            }
        }
        private void LoadStatusView (Order order, OrderStatusDetail status, bool closeScreenWhenCompleted)
        {
            
            InvokeOnMainThread (() => 
                                    {
                                        _btnBookIt.Maybe (() => _btnBookIt.Hidden = true);
                                        RemoveStatusView();
                                         _statusView = new StatusView (this, order, status, closeScreenWhenCompleted);
                                        _statusView.CloseRequested += delegate(object sender, EventArgs e) 
                                                                            {
                                                                                RemoveStatusView();
                                                                                AppContext.Current.LastOrder = null;
                                                                                Selected ();
                                                                             };
                                       _statusView.View.Frame = scrollView.Frame;
                                        this.View.AddSubview (_statusView.View);
                                    });
            
           
        }

        void StatusViewCloseRequested (object sender, EventArgs e)
        {
            
        }

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
                
                if (_pickup == null)
                {
                    Console.WriteLine ("PICKUP NULL!!!!");
                }
                else
                {
                    Console.WriteLine ("pickup not null!!!");
                }
            
                
                if (AppContext.Current.LastOrder.HasValue) 
                {
                    scrollView.Hidden = true;
                    LoadStatusView (true);
                }
                else
                {
                    scrollView.Hidden = false;
                    _btnBookIt.Maybe (() => _btnBookIt.Hidden = false);
                    
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
                    
                    
                    
                    if (pageControl != null)
                    {
                        pageControl.CurrentPage = 0;
                        PointF p = new PointF (0, 0);
                        scrollView.SetContentOffset (p, true);
                    }
                    _pickup.Maybe (() => _pickup.AssignData ());
                    _destination.Maybe (() => _destination.AssignData ());
                    
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
            _pickup.Maybe (() => _pickup.RefreshData ());
            _destination.Maybe (() => _destination.RefreshData ());
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
                    _pickup.SuspendRegionChanged ();
                    _destination.SuspendRegionChanged ();
                    _pickup.PrepareData ();
                    _destination.PrepareData ();
                    
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
                        _pickup.ResumeRegionChanged ();
                        _destination.ResumeRegionChanged ();
                        LoadingOverlay.StopAnimatingLoading (this.View);
                        View.UserInteractionEnabled = true;
                    });
                }
                
            });
        }

        public bool IsTopView {

            get { return this.NavigationController.TopViewController is BookTabView; }
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

