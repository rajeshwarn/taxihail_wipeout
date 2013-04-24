using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Configuration;
using System.Globalization;
using System.Reactive.Linq;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;
using apcurium.MK.Common;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookingStatusViewModel : BaseViewModel
    {

		private const int _refreshPeriod = 20 ; //20 sec

        private bool _isThankYouDialogDisplayed = false;
        private bool _hasSeenReminder = false;


		public BookingStatusViewModel (string order, string orderStatus)
		{
			Order = JsonSerializer.DeserializeFromString<Order> (order);
			OrderStatusDetail = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);      

			_hasSeenReminder = false;
		}
	
		public override void Load ()
        {
			base.Load ();
			ShowRatingButton = true;


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

		public override void Start (bool firstStart)
		{
			base.Start (firstStart);

			Observable.Timer ( TimeSpan.FromSeconds ( 2 ), TimeSpan.FromSeconds (_refreshPeriod))
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
		
		BookAddressViewModel dropoffViewModel;
		public BookAddressViewModel Dropoff {
			get {
				return dropoffViewModel;
			}
			set {
				dropoffViewModel = value;
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
		
		public bool IsCallButtonVisible {
			get { return !bool.Parse (TinyIoCContainer.Current.Resolve<IConfigurationManager> ().GetSetting ("Client.HideCallDispatchButton")); }
			private set{}
		}

		private string _statusInfoText { get; set; }
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
			}
		}

		private bool _showRatingButton;
		public bool ShowRatingButton {
			get { 
				if (!TinyIoCContainer.Current.Resolve<IAppSettings> ().RatingEnabled) {
					return false;
				} else {
					return _showRatingButton;
				}
			}
			set { 
				_showRatingButton = value;
				FirePropertyChanged (() => ShowRatingButton);
			}
		}

		#endregion



        private void HideRatingButton (OrderRated orderRated)
        {
            ShowRatingButton = false;
            ShowThankYouDialog ();
        }

        private void AddReminder (OrderStatusDetail status)
        {
			if (!_hasSeenReminder
				&& this.PhoneService.CanUseCalendarAPI())
            {
                this._hasSeenReminder = true;
                InvokeOnMainThread (() => 
                {
					MessageService.ShowMessage (Str.AddReminderTitle, Str.AddReminderMessage, Str.YesButtonText, () => 
                    {
						this.PhoneService.AddEventToCalendarAndReminder(Str.ReminderTitle, 
						                                                Str.GetReminderDetails(Order.PickupAddress.FullAddress, Order.PickupDate),						              									 
                                                                        Order.PickupAddress.FullAddress, 
                                                                        Order.PickupDate, 
                                                                        Order.PickupDate.AddHours(-2));
                    }, Str.NoButtonText, () => { });
                });
            }
        }

        
		string vehicleNumber = null;
        private void RefreshStatus ()
        {

            try {
                var status = BookingService.GetOrderStatus (Order.Id);
				if(status.VehicleNumber != null)
				{
					vehicleNumber = status.VehicleNumber;
				}
				else{
					status.VehicleNumber = vehicleNumber;
				}

				var isDone = BookingService.IsStatusDone (status.IBSStatusId);


				if(status.IBSStatusId.Equals(VehicleStatuses.Common.Scheduled) )
				{
					AddReminder(status);
				}

#if DEBUG
                status.IBSStatusId = VehicleStatuses.Common.Arrived;
#endif

                if (status != null) {
                    StatusInfoText = status.IBSStatusDescription;                        
                    this.OrderStatusDetail = status;

                    CenterMap ();

					UpdatePayCancelButtons(status.IBSStatusId);

                    if (OrderStatusDetail.IBSOrderId.HasValue) {
						ConfirmationNoTxt = Str.GetStatusDescription(OrderStatusDetail.IBSOrderId.Value+"");
                    }

                    if (isDone) {
                        if (!_isThankYouDialogDisplayed) {
                            _isThankYouDialogDisplayed = true;
                            InvokeOnMainThread (ShowThankYouDialog);
                        }
                    }
                }
            } catch (Exception ex) {
                TinyIoCContainer.Current.Resolve<ILogger> ().LogError (ex);
            }
        }

		void UpdatePayCancelButtons (string statusId)
		{
			IsPayButtonVisible = statusId == VehicleStatuses.Common.Arrived
					||statusId == VehicleStatuses.Common.Done
					||statusId == VehicleStatuses.Common.Loaded;
			
			IsCancelButtonVisible = !IsPayButtonVisible;

            //if (!Settings.PayByCreditCardEnabled) {
                IsPayButtonVisible = false;
            //}
		}

        private void ShowThankYouDialog ()
        {
			var hub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub> ();
            TinyMessageSubscriptionToken orderRatedToken = null;

			Action showRating = () =>
			{
				if ((Common.Extensions.GuidExtensions.HasValue (Order.Id))) {
					Order.Id = Order.Id;
					orderRatedToken = hub.Subscribe<OrderRated> (HideRatingButton);
					NavigateToRatingPage.Execute ();
				}
			};

			Action closeAction = () =>
			{
				if (orderRatedToken != null) {
					hub.Unsubscribe<OrderRated> (orderRatedToken);
				}
				this.Close ();
			};

			Action sendRecieptAction = () =>
			{
				if (Common.Extensions.GuidExtensions.HasValue (Order.Id)) {
					BookingService.SendReceipt (Order.Id);
				}
			};

			string stringNeutral = ShowRatingButton ? Str.RateButtonText : null;
			Action actionNeutral = ShowRatingButton ?  showRating : null;    


            MessageService.ShowMessage (
				Str.ThankYouTitle,
				Str.ThankYouMessage,
				Str.ReturnBookingScreenMessage, 
				closeAction,
                Str.HistoryDetailSendReceiptButtonText, 
				sendRecieptAction,
                stringNeutral, actionNeutral
            );
        }


        private void CenterMap ()
        {            
			var pickup = CoordinateViewModel.Create(Pickup.Model.Latitude, Pickup.Model.Longitude, true);
            if (OrderStatusDetail.VehicleLatitude.HasValue && OrderStatusDetail.VehicleLongitude.HasValue) 
			{
                MapCenter = new CoordinateViewModel[] 
				{ 
					pickup,
					CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value)                   
                };
            } else {
                MapCenter = new CoordinateViewModel[] { pickup };
            }
        }

		#region Commands
        public IMvxCommand NavigateToRatingPage {
            get {
                return GetCommand (() =>
                {
                    MessengerHub.Subscribe<OrderRated> (HideRatingButton);
                    RequestNavigate<BookRatingViewModel> (new { orderId = Order.Id.ToString (), canRate = true.ToString (CultureInfo.InvariantCulture), isFromStatus = true.ToString (CultureInfo.InvariantCulture) });
                });
            }
        }

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
					if ((OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Done) || (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded)) {
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
	                                MessengerHub.Publish (new OrderCanceled (this, Order, null));
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
                                               Settings.PhoneNumberDisplay (Order.Settings.ProviderId.Value), 
                                               Str.CallButtonText, 
					                           () => PhoneService.Call (Settings.PhoneNumber (Order.Settings.ProviderId.Value)),
						                       Str.CancelButtonText, 
                                               () => {});                    
                });
            }
        }
		#endregion
    }
}
