using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration;
using System.Reactive.Linq;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;
using apcurium.MK.Common;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class BookingStatusViewModel : BaseViewModel, IMvxServiceConsumer<IBookingService>
    {
		private readonly IBookingService _bookingService;
        private int _refreshPeriod = 5; //in seconds
	    private bool _waitingToNavigateAfterTimeOut;

		public BookingStatusViewModel (string order, string orderStatus)
		{
			Order = JsonSerializer.DeserializeFromString<Order> (order);
			OrderStatusDetail = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);      
            IsCancelButtonVisible = true;			
            _waitingToNavigateAfterTimeOut = false;
			_bookingService = this.GetService<IBookingService>();
		}
	
		public override void Load ()
        {
			base.Load ();

			StatusInfoText = Str.GetStatusInfoText(Str.LoadingMessage);

            Pickup = new BookAddressViewModel (() => Order.PickupAddress, address => Order.PickupAddress = address)
            {
				EmptyAddressPlaceholder = Str.BookPickupLocationEmptyPlaceholder
            };

            Dropoff = new BookAddressViewModel (() => Order.DropOffAddress, address => Order.DropOffAddress = address)
            {
				EmptyAddressPlaceholder = Str.BookPickupLocationEmptyPlaceholder
            };

            CenterMap ();
        }

		public override void Start (bool firstStart = false)
		{
			base.Start (firstStart);

            var periodInSettings = TinyIoCContainer.Current.Resolve<IConfigurationManager> ().GetSetting ("Client.OrderStatus.ClientPollingInterval");
            int periodInSettingsValue;
            if(int.TryParse(periodInSettings, out periodInSettingsValue))
            {
                _refreshPeriod = periodInSettingsValue;
            }

            Task.Factory.StartNew (() =>
            {
                Thread.Sleep( 1000 );     
                InvokeOnMainThread(RefreshStatus);
            });

			Observable.Interval( TimeSpan.FromSeconds (_refreshPeriod))
				.Subscribe (unit => InvokeOnMainThread (RefreshStatus))
				.DisposeWith (Subscriptions);

		}
		
		protected readonly CompositeDisposable Subscriptions = new CompositeDisposable ();
		public override void Stop ()
		{
			base.Stop ();
            Subscriptions.DisposeAll ();
		}

		#region Bindings
		private IEnumerable<CoordinateViewModel> _mapCenter;
		public IEnumerable<CoordinateViewModel> MapCenter {
			get { return _mapCenter; }
			private set {
				_mapCenter = value;
				FirePropertyChanged (() => MapCenter);
			}
		}
		
		BookAddressViewModel _pickupViewModel;
		public BookAddressViewModel Pickup {
			get {
				return _pickupViewModel;
			}
			set {
				_pickupViewModel = value;
				FirePropertyChanged (() => Pickup); 
			}
		}
		
		BookAddressViewModel _dropoffViewModel;
		public BookAddressViewModel Dropoff {
			get {
				return _dropoffViewModel;
			}
			set {
				_dropoffViewModel = value;
				FirePropertyChanged (() => Dropoff); 
			}
		}

		bool _isPayButtonVisible;
		public bool IsPayButtonVisible {
			get{
				return _isPayButtonVisible;
			} set{
				_isPayButtonVisible = value;
				FirePropertyChanged (() => IsPayButtonVisible); 
			}
		}

		bool _isCancelButtonVisible;
		public bool IsCancelButtonVisible{
			get{
				return _isCancelButtonVisible;
			} set{
				_isCancelButtonVisible = value;
				FirePropertyChanged (() => IsCancelButtonVisible); 
			}
		}
		
        bool _isResendButtonVisible;
        public bool IsResendButtonVisible{
            get{
                return _isResendButtonVisible;
            } set{
                _isResendButtonVisible = value;
                FirePropertyChanged (() => IsResendButtonVisible); 
            }
        }

        private string _confirmationNoTxt;
		public string ConfirmationNoTxt {
			get {
				return _confirmationNoTxt;
			}
			set {
				_confirmationNoTxt = value;
				FirePropertyChanged (() => ConfirmationNoTxt);
			}
		}
        public bool IsCallTaxiVisible
        {
            get { 
                    var showCallDriver = Config.GetSetting("Client.ShowCallDriver", false);
                return showCallDriver && IsDriverInfoAvailable && OrderStatusDetail.DriverInfos.MobilePhone.HasValue (); }
        }

        public bool IsDriverInfoAvailable
        {
            get { 
                var showVehicleInformation = Config.GetSetting("Client.ShowVehicleInformation", true);

				return showVehicleInformation && ( (OrderStatusDetail.IbsStatusId == VehicleStatuses.Common.Assigned) || (OrderStatusDetail.IbsStatusId == VehicleStatuses.Common.Arrived) ) 
                && ( OrderStatusDetail.DriverInfos.VehicleRegistration.HasValue() || OrderStatusDetail.DriverInfos.LastName.HasValue() || OrderStatusDetail.DriverInfos.FirstName.HasValue()); }
        }
		
		public bool IsCallButtonVisible {
            get { return !bool.Parse (Config.GetSetting ("Client.HideCallDispatchButton")); }
		}

		public bool VehicleDriverHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.FullName) || !IsDriverInfoAvailable; }
		}
		public bool VehicleLicenceHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleRegistration) || !IsDriverInfoAvailable; }
		}
		public bool VehicleTypeHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleType) || !IsDriverInfoAvailable; }
		}
		public bool VehicleMakeHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleMake) || !IsDriverInfoAvailable; }
		}
		public bool VehicleModelHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleModel) || !IsDriverInfoAvailable; }
		}
		public bool VehicleColorHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleColor) || !IsDriverInfoAvailable; }
		}

	    private string _statusInfoText;
		public string StatusInfoText {
			get { return _statusInfoText; }
			set {
				_statusInfoText = value;
				FirePropertyChanged (() => StatusInfoText);
			}
		}

		public Address PickupModel {
			get { return Pickup.Model; }
			set {
				Pickup.Model = value;
				FirePropertyChanged (() => PickupModel);
			}
		}

		private Order _order;
		public Order Order {
			get { return _order; }
			set {
				_order = value;
				FirePropertyChanged (() => Order);
			}
		}
		
		private OrderStatusDetail _orderStatusDetail;
		public OrderStatusDetail OrderStatusDetail {
			get { return _orderStatusDetail; }
			set {
				_orderStatusDetail = value;
				FirePropertyChanged (() => OrderStatusDetail);
				FirePropertyChanged (() => VehicleDriverHidden);
				FirePropertyChanged (() => VehicleLicenceHidden);
				FirePropertyChanged (() => VehicleTypeHidden);
				FirePropertyChanged (() => VehicleMakeHidden);
				FirePropertyChanged (() => VehicleModelHidden);
				FirePropertyChanged (() => VehicleColorHidden);
                FirePropertyChanged (() => IsDriverInfoAvailable);
                FirePropertyChanged (() => IsCallTaxiVisible);
			}
		}

        public IMvxCommand CallTaxi
        {
            get { 
				return GetCommand(() =>
                {
                    if (!string.IsNullOrEmpty(OrderStatusDetail.DriverInfos.MobilePhone))
                    {
						MessageService.ShowMessage (string.Empty, 
						                            OrderStatusDetail.DriverInfos.MobilePhone, 
						                            Str.CallButtonText, 
						                            () => PhoneService.Call (OrderStatusDetail.DriverInfos.MobilePhone),
						                            Str.CancelButtonText, 
						                            () => {});   
                    }
                    else
                    {
                        MessageService.ShowMessage(Resources.GetString("NoPhoneNumberTitle"), Resources.GetString("NoPhoneNumberMessage"));
                    }
                }); 
			}
        }

		#endregion


        private bool HasSeenReminderPrompt( Guid orderId )
        {
            var hasSeen = CacheService.Get<string>( "OrderReminderWasSeen." + orderId.ToString());
            return !string.IsNullOrEmpty(hasSeen);
        }
        private void SetHasSeenReminderPrompt( Guid orderId )
        {
            CacheService.Set( "OrderReminderWasSeen." + orderId.ToString(), true.ToString() );                     
        }




        private void AddReminder (OrderStatusDetail status)
        {
            if (!HasSeenReminderPrompt(status.OrderId )
				&& PhoneService.CanUseCalendarAPI())
            {
                SetHasSeenReminderPrompt(status.OrderId);
                InvokeOnMainThread (() => MessageService.ShowMessage (Str.AddReminderTitle, Str.AddReminderMessage, Str.YesButtonText, () => PhoneService.AddEventToCalendarAndReminder(Str.ReminderTitle, 
                    Str.GetReminderDetails(Order.PickupAddress.FullAddress, Order.PickupDate),						              									 
                    Order.PickupAddress.FullAddress, 
                    Order.PickupDate, 
                    Order.PickupDate.AddHours(-2)), Str.NoButtonText, () => { }));
            }
        }
		        
		string _vehicleNumber;
        private void RefreshStatus ()
        {
            try {
                var status = BookingService.GetOrderStatus (Order.Id);
				if(status.VehicleNumber != null)
				{
					_vehicleNumber = status.VehicleNumber;
				}
				else{
					status.VehicleNumber = _vehicleNumber;
				}

				var isDone = BookingService.IsStatusDone (status.IbsStatusId);

				if(status.IbsStatusId.HasValue() && status.IbsStatusId.Equals(VehicleStatuses.Common.Scheduled) )
				{
					AddReminder(status);
				}

#if DEBUG
                //status.IBSStatusId = VehicleStatuses.Common.Arrived;
#endif
                IsPayButtonVisible = false;
                StatusInfoText = status.IbsStatusDescription;                        
                OrderStatusDetail = status;

                CenterMap ();

                UpdatePayCancelButtons(status.IbsStatusId);

                if (OrderStatusDetail.IbsOrderId.HasValue) {
                    ConfirmationNoTxt = Str.GetStatusDescription(OrderStatusDetail.IbsOrderId.Value+"");
                }

                if (isDone) 
                {
                    GoToSummary();
                }

                if (BookingService.IsStatusTimedOut (status.IbsStatusId))
                {
                    GoToBookingScreen();
                }
            } catch (Exception ex) {
                Logger.LogError (ex);
            }
        }

		void UpdatePayCancelButtons (string statusId)
		{
            var setting = ConfigurationManager.GetPaymentSettings ();
            var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;

			IsPayButtonVisible =  (statusId == VehicleStatuses.Common.Done
								||statusId == VehicleStatuses.Common.Loaded) 
                                && (isPayEnabled && !PaymentService.GetPaymentFromCache(Order.Id).HasValue);
			
            IsCancelButtonVisible = statusId == null ||
                                    statusId == VehicleStatuses.Common.Assigned 
                                || statusId == VehicleStatuses.Common.Waiting 
                                || statusId == VehicleStatuses.Common.Arrived
                                || statusId == VehicleStatuses.Common.Scheduled;

            IsResendButtonVisible = isPayEnabled && PaymentService.GetPaymentFromCache(Order.Id).HasValue;
		}

		public void GoToSummary(){

			RequestNavigate<RideSummaryViewModel> (new {
				order = Order.ToJson(),
				orderStatus = OrderStatusDetail.ToJson()
			}.ToStringDictionary());
			RequestClose (this);
		}

        public void GoToBookingScreen(){
            if (!_waitingToNavigateAfterTimeOut)
            {
				Observable.Interval( TimeSpan.FromSeconds (10))
                    .Subscribe(unit => InvokeOnMainThread(() => {
                        _bookingService.ClearLastOrder();
                        _waitingToNavigateAfterTimeOut = true;
                        RequestNavigate<BookViewModel>(clearTop: true);
                        RequestClose(this);
                    }));
            }
        }

        private void CenterMap ()
        {            
			var pickup = CoordinateViewModel.Create(Pickup.Model.Latitude, Pickup.Model.Longitude, true);
			if (OrderStatusDetail.IbsStatusId != VehicleStatuses.Common.Waiting && OrderStatusDetail.VehicleLatitude.HasValue && OrderStatusDetail.VehicleLongitude.HasValue) 
			{
                MapCenter = new[] 
				{ 
					pickup,
					CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value)                   
                };
            } else {
                MapCenter = new[] { pickup };
            }
        }

		#region Commands

        public IMvxCommand NewRide {
            get {
                return GetCommand (() =>
                {
                    MessageService.ShowMessage (Str.StatusNewRideButtonText, Str.StatusConfirmNewBooking, Str.YesButtonText, () =>
                    {
                        BookingService.ClearLastOrder ();
                        RequestNavigate<BookViewModel> (clearTop: true);
                    },
                    Str.NoButtonText, NoAction);                    
                });
            }
        }

        public IMvxCommand CancelOrder {
            get {
                return GetCommand (() =>
                {
						if ((OrderStatusDetail.IbsStatusId == VehicleStatuses.Common.Done) || (OrderStatusDetail.IbsStatusId == VehicleStatuses.Common.Loaded)) {
                        MessageService.ShowMessage (Str.CannotCancelOrderTitle, Str.CannotCancelOrderMessage);
                        return;
                    }

                    MessageService.ShowMessage ("", Str.StatusConfirmCancelRide, Str.YesButtonText, ()=>
                    {
                        Task.Factory.SafeStartNew ( () =>
                        {
	                        try 
	                        {
	                            MessageService.ShowProgress (true);

	                            var isSuccess = BookingService.CancelOrder (Order.Id);      
	                            if (isSuccess) 
	                            {
									_bookingService.ClearLastOrder();
	                                RequestNavigate<BookViewModel> (clearTop: true);
	                            } 
	                            else 
	                            {
	                                MessageService.ShowMessage (Str.StatusConfirmCancelRideErrorTitle, Str.StatusConfirmCancelRideError);
	                            }
	                        } 
	                        finally 
	                        {
	                            MessageService.ShowProgress (false);
	                        }     
                        });
                    },Str.NoButtonText, () => { });
                });
            }
        }

		public IMvxCommand PayForOrderCommand 
		{
			get {
				return GetCommand (() =>
					{ 
#if DEBUG
#else
                        if(string.IsNullOrWhiteSpace(OrderStatusDetail.VehicleNumber)){
                            MessageService.ShowMessage(Resources.GetString("VehicleNumberErrorTitle"), Resources.GetString("VehicleNumberErrorMessage"));
                            return;
                       }
#endif

						RequestNavigate<ConfirmCarNumberViewModel>(
						new 
						{ 
							order = Order.ToJson(),
							orderStatus = OrderStatusDetail.ToJson()
						}, false, MvxRequestedBy.UserAction);
					});
			}
		}

        public IMvxCommand CallCompany {
            get {
                return GetCommand (() =>
                {                    
                    MessageService.ShowMessage (string.Empty, 
                                                Config.GetSetting( "DefaultPhoneNumberDisplay" ),
                                               Str.CallButtonText, 
                                                () => PhoneService.Call ( Config.GetSetting( "DefaultPhoneNumber" )),
						                       Str.CancelButtonText, 
                                               () => {});                    
                });
            }
        }

       public IMvxCommand ResendConfirmationToDriver {
            get {
                return GetCommand (() =>
                                   {
                    if( PaymentService.GetPaymentFromCache(Order.Id).HasValue )
                    {
                        PaymentService.ResendConfirmationToDriver( Order.Id );
                    }
                });
            }
        }
		#endregion
    }
}
