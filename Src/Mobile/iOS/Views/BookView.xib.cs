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
 
namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class BookView : MvxBindingTouchViewController<BookViewModel>, ITaxiViewController, ISelectableViewController, IRefreshableViewController
    {
        #region Constructors

        public event EventHandler TabSelected;

        //private CreateOrder _bookingInfo;
        private StatusView _statusView;
        //private VerticalButtonBar _settingsBar;
        public BookView() 
            : base(new MvxShowViewModelRequest<BookViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }

        protected BookView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        protected BookView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }

//        public BookView(IntPtr handle) : base(  base(handle)
//        {
//            Initialize();
//        }
//
//        [Export("initWithCoder:")]
//        public BookView(NSCoder coder) : base(coder)
//        {
//            Initialize();
//        }
//
//        public BookView() : base("BookView", null)
//        {
//            Initialize();
//        }



       

        
        public CreateOrder BookingInfo
        {
            get { return ViewModel.Order; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png")); 

            AppButtons.FormatStandardButton((GradientButton)refreshCurrentLocationButton, "", AppStyle.ButtonColor.Blue, "");
            AppButtons.FormatStandardButton((GradientButton)bookLaterButton, "", AppStyle.ButtonColor.DarkGray );

            AppButtons.FormatStandardButton((GradientButton)dropoffButton, "Dropoff", AppStyle.ButtonColor.Grey, "");
            AppButtons.FormatStandardButton((GradientButton)pickupButton, "Pickup", AppStyle.ButtonColor.Grey );

            AppButtons.FormatStandardButton((GradientButton)dropoffActivationButton, "", AppStyle.ButtonColor.Grey, "");
            AppButtons.FormatStandardButton((GradientButton)pickupActivationButton, "", AppStyle.ButtonColor.Grey );
            ((GradientButton)dropoffActivationButton).SetImage( "Assets/location.png" );
            ((GradientButton)pickupActivationButton).SetImage( "Assets/location.png" );

            ((GradientButton)dropoffActivationButton).SetSelectedImage( "Assets/locationSelected.png" );
            ((GradientButton)pickupActivationButton).SetSelectedImage( "Assets/locationSelected.png" );


            headerBackgroundView.Image =UIImage.FromFile("Assets/backPickupDestination.png");

            ((GradientButton)bookLaterButton).SetImage( "Assets/bookLaterIcon.png" );
            ((GradientButton)refreshCurrentLocationButton).SetImage( "Assets/gpsRefreshIcon.png" );

            AppButtons.FormatStandardButton((GradientButton)bookBtn, Resources.BookItButton, AppStyle.ButtonColor.Green);
            bookBtn.TouchUpInside += BookitButtonTouchUpInside;


            UIImageView img = new UIImageView(UIImage.FromFile("Assets/location.png"));
            img.BackgroundColor = UIColor.Clear;
            img.Frame = new System.Drawing.RectangleF(mapView.Frame.X + ((mapView.Frame.Width / 2) - 10), mapView.Frame.Y + ((mapView.Frame.Height / 2)) - 30, 20, 20);
            mapView.Superview.AddSubview(img);
            mapView.MultipleTouchEnabled = true;

            bottomBar.UserInteractionEnabled = true;
            View.BringSubviewToFront(bottomBar);
            View.BringSubviewToFront(bookBtn);

//            { dropoffActivationButton, ""},              
//            { pickupActivationButton, "{'Selected':{'Path':'PickupIsActive'}}"},                


            this.AddBindings(new Dictionary<object, string>()                            {
                { refreshCurrentLocationButton, "{'TouchUpInside':{'Path':'RequestCurrentLocationCommand'}}"},                
                { pickupActivationButton, "{'TouchUpInside':{'Path':'ActivatePickup'},'Selected':{'Path':'PickupIsActive', 'Mode':'TwoWay'}}"},                
                { dropoffActivationButton, "{'TouchUpInside':{'Path':'ActivateDropoff'}}"},                


            });
        }

       

        void HandleTouchUpInside(object sender, EventArgs e)
        {           
            //_settingsBar._mainBtn.Frame = new RectangleF(0, 50, 40, 33);
        }

        void HandleSettingsButtonClicked(int index)
        {
//          InvokeOnMainThread (() => {
//              switch( index )
//              {
//              case 0:
//                  AppContext.Current.Controller.SelectViewController( 3 );
//                  break;
//              case 1:
//                  AppContext.Current.Controller.SelectViewController( 2 );
//                  break;
//              case 2:
//                  AppContext.Current.Controller.SelectViewController( 1 );
//                  break;
//              }
//
//          });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);     

            AppContext.Current.ReceiveMemoryWarning = false;              
        }
        
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            
            if ((AppContext.Current.LastOrder.HasValue) && (AppContext.Current.LoggedUser != null))
            {
                LoadStatusView(true);
            }
        }

        public string GetTitle()
        {
            return "";
        }
        
        void BookitButtonTouchUpInside(object sender, EventArgs e)
        {
            BookTaxi();
        }

        private CreateOrder _toRebookData;

        public void Rebook(Order data)
        {
            _toRebookData = JsonSerializer.DeserializeFromString<CreateOrder>(JsonSerializer.SerializeToString<Order>(data));
            _toRebookData.Id = Guid.Empty;
            _toRebookData.PickupDate = null;
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
                {
                }
            }
        }

        private void LoadStatusView(Order order, OrderStatusDetail status, bool closeScreenWhenCompleted)
        {
            InvokeOnMainThread(() => {
                RemoveStatusView();
                _statusView = new StatusView(this, order, status, closeScreenWhenCompleted);
                _statusView.HidesBottomBarWhenPushed = true;
                _statusView.CloseRequested += delegate(object sender, EventArgs e)
                {
                    RemoveStatusView();
                    AppContext.Current.LastOrder = null;
                    NavigationController.TabBarController.TabBar.Hidden = true;
                    Selected();
                };

                NavigationController.PushViewController(_statusView, true);
            }); 
        }

        void StatusViewCloseRequested(object sender, EventArgs e)
        {
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

        public void Selected()
        {
            try
            {
                AppContext.Current.ResetPosition();

                if (AppContext.Current.LastOrder.HasValue)
                {
                    LoadStatusView(true);
                }
                else
                {  
                    if (!(this.NavigationController.TopViewController is BookView))
                    {
                        this.NavigationController.PopViewControllerAnimated(false);
                    }
                    
                    bool isRebook = false;
                    if (_toRebookData != null)
                    {                       
                        ViewModel.Rebook(_toRebookData);
                        isRebook = true;
                        _toRebookData = null;
                        BookTaxi();
                    }
                    else
                    {
                        ViewModel.Reset();
                    }

                    
                    if ((!isRebook) && (TabSelected != null))
                    {
                        TabSelected(this, EventArgs.Empty);
                    }
                }
            }
            catch
            {
            }
        }

        public void RefreshData()
        {
            Selected();
        }

        public void BookTaxi()
        {
            if (BookingInfo == null)
            {
                return;
            }

            if (BookingInfo.Settings.Passengers == 0)
            {
                BookingInfo.Settings = AppContext.Current.LoggedUser.Settings;
            }

            LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
            View.UserInteractionEnabled = false;
            
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
                        var view = new ConfirmationView(this);
                        view.HidesBottomBarWhenPushed = true;
                        this.NavigationController.PushViewController(view, true);
                        
                        view.Canceled += delegate(object sender, EventArgs e)
                        {
                            this.NavigationController.PopViewControllerAnimated(true);
                        };
                        
                        view.Confirmed += delegate(object sender, EventArgs e)
                        {
                            CreateOrder(view.BI);
                        };

                        view.NoteChanged += delegate(string note)
                        {
                            BookingInfo.Note = note;
                        };
                    });
                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        LoadingOverlay.StopAnimatingLoading(this.View);
                        View.UserInteractionEnabled = true;
                    });
                }
                
            });
        }

        public bool IsTopView
        {

            get { return this.NavigationController.TopViewController is BookView; }
        }

        public UIView GetTopView()
        {
            return null;
        }

        private void CreateOrder(CreateOrder bi)
        {
            if (!(this.NavigationController.TopViewController is BookView))
            {
                this.NavigationController.PopViewControllerAnimated(false);
            }
            
            LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
            View.UserInteractionEnabled = false;
            
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    var service = TinyIoCContainer.Current.Resolve<IBookingService>();
                    
                    string error;
                    
//                    if (!AppSettings.ShowNumberOfTaxi)
//                    {                    
                    bi.Settings.NumberOfTaxi = 1;
                    //}
                

                    bi.Id = Guid.NewGuid();

                    var orderStatus = service.CreateOrder(bi);
                    orderStatus.OrderId = bi.Id;

                    if (orderStatus.IBSOrderId.HasValue)
                    {
                        AppContext.Current.LastOrder = orderStatus.OrderId;

                        //var order = TinyIoCContainer.Current.Resolve<IAccountService>( ).GetHistoryOrder( orderStatus.OrderId );

                        var accountId = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount.Id;

                        BookingInfo.Id = orderStatus.OrderId;
                        
                        BookingInfo.PickupDate = DateTime.Now;                       
                        
                        LoadStatusView(new Order { Id = bi.Id, IBSOrderId = orderStatus.IBSOrderId,  CreatedDate = DateTime.Now, DropOffAddress = bi.DropOffAddress, PickupAddress  = bi.PickupAddress , Settings = bi.Settings   }, orderStatus, false);
                    }
                    else
                    {
                        if (error.HasValue())
                        {
                            MessageHelper.Show(Resources.ErrorCreatingOrderTitle, Resources.ErrorCreatingOrderMessage);
                        }
                        else
                        {
                            MessageHelper.Show(Resources.ErrorCreatingOrderTitle, Resources.ErrorCreatingOrderMessage);
                        }
                    }   
                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        LoadingOverlay.StopAnimatingLoading(this.View);
                        View.UserInteractionEnabled = true;
                    });
                }
            });
            
        }

        #endregion
    }
}

