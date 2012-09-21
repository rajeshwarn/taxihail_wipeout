
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.MapUtilities;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class StatusView : UIViewController
    {
        public event EventHandler CloseRequested;

        private bool _closeScreenWhenCompleted;
        private AddressAnnotation _taxiPosition;
        private NSTimer _timer;
        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code

        public StatusView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public StatusView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public StatusView(BookView parent, Order info, OrderStatusDetail status, bool closeScreenWhenCompleted) : base("StatusView", null)
        {
            _closeScreenWhenCompleted = closeScreenWhenCompleted;
            Order = info;
            Status = status;
            Initialize();

        }

        void Initialize()
        {
        }

        protected Order Order { get; set; }

        protected OrderStatusDetail Status { get; set; }

//        public void Refresh(Order order, OrderStatusDetail status, bool closeScreenWhenCompleted)
//        {
//            _closeScreenWhenCompleted = closeScreenWhenCompleted;
//            Order = order;
//            Status = status;                     
//
//
//
//            RefreshStatusDisplay();
//           
//
//        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            LoadOrderInfo();
            RefreshStatusDisplay();

            if (_timer == null)
            {           
                _timer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(4), RefreshStatus);
            }

        }

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = false;
		}

        public override void ViewDidLoad()
        {
            
            try
            {
                NavigationItem.HidesBackButton = true;                
                View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

				View.BringSubviewToFront( lblTitle );
				lblTitle.Text = Resources.LoadingMessage;

                btnChangeBooking.SetTitle(Resources.ChangeBookingSettingsButton, UIControlState.Normal);
             
				AppButtons.FormatStandardButton((GradientButton)btnCall, Resources.StatusCallButton, AppStyle.ButtonColor.Black );
				AppButtons.FormatStandardButton((GradientButton)btnCancel, Resources.StatusCancelButton, AppStyle.ButtonColor.Red );
				AppButtons.FormatStandardButton((GradientButton)btnNewRide, Resources.StatusNewRideButton, AppStyle.ButtonColor.Green );

                btnCall.TouchUpInside += CallProvider;
				btnCancel.TouchUpInside += CancelOrder;
				btnNewRide.TouchUpInside += Rebook;
                
                mapStatus.Delegate = new AddressMapDelegate();
                
//                var view = AppContext.Current.Controller.GetTitleView(null, Resources.StatusViewTitle);
                
				this.NavigationItem.TitleView = new TitleView(null, Resources.StatusViewTitle);

				View.BringSubviewToFront( bottomBar );
            
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        void Rebook (object sender, EventArgs e)
        {
			var newBooking = new Confirmation();
            newBooking.Action(Resources.StatusConfirmNewBooking, () =>
            {
                _timer.Dispose();
                _timer = null;
                if (CloseRequested != null)
                {
                    InvokeOnMainThread(() => CloseRequested(this, EventArgs.Empty));
                }
            }
            );
        }

        void CancelOrder (object sender, EventArgs e)
        {
            var newBooking = new Confirmation();
            newBooking.Action(Resources.StatusConfirmCancelRide, () => 
            {
                LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
                View.UserInteractionEnabled = false;
                ThreadHelper.ExecuteInThread(() => 
                {
                    try
                    {
                        var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(Order.Id);
                        if (isSuccess)
                        {
                            _timer.Dispose();
                            _timer = null;
                            if (CloseRequested != null)
                            {
                                InvokeOnMainThread(() => CloseRequested(this, EventArgs.Empty));
                            }
                        }
                        else
                        {
                            MessageHelper.Show(Resources.StatusConfirmCancelRideErrorTitle, Resources.StatusConfirmCancelRideError);
                        }
                    }
                    catch
                    {
                         
                        MessageHelper.Show(Resources.StatusConfirmCancelRideErrorTitle, Resources.StatusConfirmCancelRideError);
                        AppContext.Current.LastOrder = null;
                        InvokeOnMainThread(() => CloseRequested(this, EventArgs.Empty));

                    }
                    finally
                    {
                        InvokeOnMainThread(() => 
                        {
                            LoadingOverlay.StopAnimatingLoading(this.View);
                            View.UserInteractionEnabled = true;
                        }
                        );
                    }
                }
                );
            }
            );
        }

        void CallProvider (object sender, EventArgs e)
        {
            if ( !Order.Settings.ProviderId.HasValue  )             
                {
                return;
            }
            var call = new Confirmation();
            call.Call(TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumber(Order.Settings.ProviderId.Value), 
                      TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumberDisplay(Order.Settings.ProviderId.Value));
        }

        private void LoadOrderInfo()
        {

            try
            {

//                lblTitle.Text = string.Format(Resources.StatusDescription, Order.IBSOrderId);

                if (mapStatus.Annotations != null)
                {
                    mapStatus.RemoveAnnotations(mapStatus.Annotations.OfType<MKAnnotation>().ToArray());
                }
            
                var pcoordinate = Order.PickupAddress.GetCoordinate();
                mapStatus.AddAnnotation(new AddressAnnotation(pcoordinate, AddressAnnotationType.Pickup, Resources.PickupMapTitle, Order.PickupAddress.FullAddress));
            
                if (Status.IBSStatusDescription.HasValue())
                {
					lblTitle.Text = Status.IBSStatusDescription;
                }
            
                if (Order.DropOffAddress.HasValidCoordinate())
                {
                
                    try
                    {                                       
                        var dcoordinate = Order.DropOffAddress.GetCoordinate();
                        mapStatus.AddAnnotation(new AddressAnnotation(dcoordinate, AddressAnnotationType.Destination, Resources.DestinationMapTitle, Order.DropOffAddress.FullAddress));
                    
                        double latDelta = Math.Abs(dcoordinate.Latitude - pcoordinate.Latitude);
                        double longDelta = Math.Abs(dcoordinate.Longitude - pcoordinate.Longitude);
                    
                        var center = new CLLocationCoordinate2D((pcoordinate.Latitude + dcoordinate.Latitude) / 2, (pcoordinate.Longitude + dcoordinate.Longitude) / 2);
                    
                        mapStatus.SetRegion(new MKCoordinateRegion(center, new MKCoordinateSpan(latDelta, longDelta)), true);
                    
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                }
                else
                {
                    mapStatus.SetRegion(new MKCoordinateRegion(pcoordinate, new MKCoordinateSpan(0.02, 0.02)), true);
                
                }
            
            }
            catch (Exception ex)
            {
                lblTitle.Text =  Resources.ErrorGettingStatus;
            }
            
        }

        private void RefreshStatusDisplay()
        {
            if ( Status == null )
            {
                 lblTitle.Text =  Resources.ErrorGettingStatus;
                return;
            }

			InvokeOnMainThread(() => lblTitle.Text = Status.IBSStatusDescription);
            if ((Status.VehicleLatitude.HasValue) && (Status.VehicleLongitude.HasValue) && (Status.VehicleLatitude.Value != 0) && (Status.VehicleLongitude.Value != 0))
            {
                CLLocationCoordinate2D position = new CLLocationCoordinate2D(Status.VehicleLatitude.Value, Status.VehicleLongitude.Value);
                if (_taxiPosition != null)
                {
                    InvokeOnMainThread(() => 
                    {
                        mapStatus.RemoveAnnotations(mapStatus.Annotations.OfType<AddressAnnotation>().Where(a => a.AddressType == AddressAnnotationType.Taxi).ToArray());
                    }
                    );
                }

                _taxiPosition = new AddressAnnotation(position, AddressAnnotationType.Taxi, Resources.TaxiMapTitle, "");
                InvokeOnMainThread(() => 
                {
                    mapStatus.AddAnnotation(_taxiPosition);
                    mapStatus.SetCenterCoordinate(position, true);
                    mapStatus.SetRegion(new MKCoordinateRegion(position, new MKCoordinateSpan(0.01, 0.01)), true);
                }
                );
            }
        }

        private void RefreshStatus()
        {
            ThreadHelper.ExecuteInThread(() =>
            {

                if ( Order == null )
                {
                    InvokeOnMainThread(() => lblTitle.Text =  Resources.ErrorGettingStatus);
                    return;
                }
                
                try
                {
                    if (_closeScreenWhenCompleted)
                    {
                        var isCompleted = TinyIoCContainer.Current.Resolve<IBookingService>().IsCompleted(Order.Id);
                        
                        //|| (_debugCoordinatesIndex == 6))
                        if (isCompleted)
                        {
                            //_debugCoordinatesIndex = 0;
                            _timer.Dispose();
                            _timer = null;
                            if (CloseRequested != null)
                            {
                                InvokeOnMainThread(() => CloseRequested(this, EventArgs.Empty));
                            }
                            return;
                        }
                        
                    }
                    
                    Console.WriteLine("Refreshing timer");
                    Status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(Order.Id);                   
                    
                    
                    Console.WriteLine("Refreshing timer - S: " + Status.IBSStatusId);
                    Console.WriteLine("Refreshing timer -LA: " + Status.VehicleLatitude.ToString());
                    Console.WriteLine("Refreshing timer -LO: " + Status.VehicleLongitude.ToString());
                    
                    Console.WriteLine("Refreshing timer -Id: " + Order.Id.ToString());
                    Console.WriteLine("Refreshing timer -Adrs: " + Order.PickupAddress.FullAddress.ToString());
                    Console.WriteLine("Refreshing timer -Adrs LO: " + Order.PickupAddress.Longitude.ToString());
                    Console.WriteLine("Refreshing timer -Adrs LA: " + Order.PickupAddress.Latitude.ToString());
                    
                    if (Status != null)
                    {                        
                        RefreshStatusDisplay();                        
                    }
                    
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
            );
        }

        void RefreshButtonTouchUpInside(object sender, EventArgs e)
        {
            
        }

        public override void ViewDidDisappear(bool animated)
        {
            try
            {
                base.ViewDidDisappear(animated);
                _timer.Dispose();

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                _timer = null;
            }
        }

        private void CancelOrder()
        {
            var newBooking = new Confirmation();
            newBooking.Action(Resources.StatusConfirmCancelRide, () => 
            {
                LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
                View.UserInteractionEnabled = false;
                ThreadHelper.ExecuteInThread(() => 
                {
                    try
                    {
                        var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(Order.Id);
                        if (isSuccess)
                        {
                            _timer.Dispose();
                            _timer = null;
                            if (CloseRequested != null)
                            {
                                InvokeOnMainThread(() => CloseRequested(this, EventArgs.Empty));
                            }
                        }
                        else
                        {
                            MessageHelper.Show(Resources.StatusConfirmCancelRideErrorTitle, Resources.StatusConfirmCancelRideError);
                        }
                    }
                    catch
                    {
                         
                        MessageHelper.Show(Resources.StatusConfirmCancelRideErrorTitle, Resources.StatusConfirmCancelRideError);
                        AppContext.Current.LastOrder = null;
                        InvokeOnMainThread(() => CloseRequested(this, EventArgs.Empty));

                    }
                    finally
                    {
                        InvokeOnMainThread(() => 
                        {
                            LoadingOverlay.StopAnimatingLoading(this.View);
                            View.UserInteractionEnabled = true;
                        }
                        );
                    }
                }
                );
            }
            );
        }

        private void Rebook()
        {
            var newBooking = new Confirmation();
            newBooking.Action(Resources.StatusConfirmNewBooking, () =>
            {
                _timer.Dispose();
                _timer = null;
                if (CloseRequested != null)
                {
                    InvokeOnMainThread(() => CloseRequested(this, EventArgs.Empty));
                }
            }
            );
        }

        private void CallProvider()
        {
            var call = new Confirmation();
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            call.Call(settings.PhoneNumber(Order.Settings.ProviderId.Value), settings.PhoneNumberDisplay(Order.Settings.ProviderId.Value));
        }

        void CallTouchUpInside(object sender, EventArgs e)
        {
            
            var actionSheet = new UIActionSheet("");
            actionSheet.AddButton(Resources.CallCompanyButton);
            actionSheet.AddButton(Resources.StatusActionBookButton);
            actionSheet.AddButton(Resources.StatusActionCancelButton);
            actionSheet.AddButton(Resources.CancelBoutton);
            actionSheet.CancelButtonIndex = 3;
            actionSheet.DestructiveButtonIndex = 2;
            actionSheet.Clicked += delegate(object se, UIButtonEventArgs ea)
            {
                
                if (ea.ButtonIndex == 0)
                {
                    CallProvider();
                }
                else if (ea.ButtonIndex == 1)
                {
                    Rebook();
                    
                }
                else if (ea.ButtonIndex == 2)
                {
                    CancelOrder();
                }
                
                
            };
            actionSheet.ShowInView(AppContext.Current.Controller.View);
            
            
            
            
        }
        
        
        #endregion
    }

//        void NewMethod()
//        {
//            var newBooking = new Confirmation();
//            newBooking.Action(Resources.StatusConfirmCancelRide, () => 
//            {
//                LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
//                View.UserInteractionEnabled = false;
//                ThreadHelper.ExecuteInThread(() => 
//                {
//                    try
//                    {
//                        var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(Order.Id);
//                        if (isSuccess)
//                        {
//                            _timer.Dispose();
//                            _timer = null;
//                            if (CloseRequested != null)
//                            {
//                                InvokeOnMainThread(() => CloseRequested(this, EventArgs.Empty));
//                            }
//                        }
//                        else
//                        {
//                            MessageHelper.Show(Resources.StatusConfirmCancelRideErrorTitle, Resources.StatusConfirmCancelRideError);
//                        }
//                    }
//                    finally
//                    {
//                        InvokeOnMainThread(() => 
//                        {
//                            LoadingOverlay.StopAnimatingLoading(this.View);
//                            View.UserInteractionEnabled = true;
//                        });
//                    }
//                });
//            });
//        }
}

