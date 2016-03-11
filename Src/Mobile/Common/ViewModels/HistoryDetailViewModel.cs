using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Threading.Tasks;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HistoryDetailViewModel : PageViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IBookingService _bookingService;
		private readonly IAccountService _accountService;
		private readonly IPaymentService _paymentSettings;

		private ClientPaymentSettings _clientPaymentSettings;

		public HistoryDetailViewModel(IOrderWorkflowService orderWorkflowService, 
			IBookingService bookingService, 
			IAccountService accountService,
			IPaymentService paymentSettings)
		{
			_orderWorkflowService = orderWorkflowService;
			_bookingService = bookingService;
			_accountService = accountService;
			_paymentSettings = paymentSettings;
		}

		public async void Init(string orderId)
		{
			Guid id;
		    if (!Guid.TryParse(orderId, out id))
		    {
		        return;
		    }

		    OrderId = id;
		    using (this.Services().Message.ShowProgress ())
		    {
		        await LoadOrder();
		        await LoadStatus();
		        _clientPaymentSettings = await _paymentSettings.GetPaymentSettings();
		        RaisePropertyChanged(() => StatusDescription); 
		    }
		}

        private Guid _orderId;
        public Guid OrderId
        {
			get { return _orderId; }
            set 
			{ 
				_orderId = value; 
				RaisePropertyChanged(); 
			}
        }

		private Order _order;
		public Order Order {
			get { return _order; }
            set 
			{ 
				_order = value; 
                if (_order.TransactionId != default(long)) 
				{
                    AuthorizationNumber = _order.TransactionId + "";
                }
				RaisePropertyChanged(() => Order); 
				RaisePropertyChanged(() => ConfirmationTxt); 
				RaisePropertyChanged(() => ShowConfirmationTxt); 
				RaisePropertyChanged(() => RequestedTxt); 
				RaisePropertyChanged(() => OriginTxt); 
				RaisePropertyChanged(() => AptRingTxt); 
				RaisePropertyChanged(() => DestinationTxt); 
				RaisePropertyChanged(() => PickUpDateTxt); 
				RaisePropertyChanged(() => PromoCode); 
            }
		}

        private OrderStatusDetail _status;
		public OrderStatusDetail Status 
        {
			get { return _status; }
		    set
		    {
		        _status = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => SendReceiptAvailable);
                RaisePropertyChanged(() => RebookIsAvailable);
		    }
		}
        
        public bool SendReceiptAvailable 
        {
            get
            {
				return (Status != null) 
						&& Status.Status == OrderStatus.Completed
						&& ( Status.FareAvailable || Status.IsManualRideLinq )
						&& Settings.SendReceiptAvailable;
            }
        }

        private bool _isDone;
        public bool IsDone
        {
			get { return _isDone; }
            set 
			{ 
				_isDone = value; 
				RaisePropertyChanged(); 
				RaisePropertyChanged(() => ShowRateButton); 
			}
        }

        private bool _isCompleted = true;
		public bool IsCompleted 
		{
			get { return _isCompleted; }
			set 
			{ 
				if (value != _isCompleted)
				{
					_isCompleted = value;
					RaisePropertyChanged ();
					RaisePropertyChanged (() => SendReceiptAvailable);
					RaisePropertyChanged (() => RebookIsAvailable);
				}
			}
		}

		public bool RebookIsAvailable
		{
			get 
			{
				return IsCompleted 
					&& !Settings.HideRebookOrder
                    && Status != null
                    && !Status.IsManualRideLinq;
			}
		}

        private bool _hasRated;
        public bool HasRated
        {
			get { return _hasRated; }
            set 
			{ 
				_hasRated = value;
				RaisePropertyChanged ();
				RaisePropertyChanged(() => ShowRateButton);  
			}
        }

        public bool ShowRateButton
        {
            get
            {          
				return Settings.RatingEnabled 
					&& IsDone 
					&& !HasRated
                    && Status != null
                    && !Status.IsManualRideLinq;
            }
        }

        private bool _canCancel;
        public bool CanCancel
        {
            get { return _canCancel; }
            set
            {
                if (value != _canCancel)
                {
                    _canCancel = value;
					RaisePropertyChanged();
                }
            }
        }

        public string ConfirmationTxt
        {
            get
            {
                if (Order != null)
                {
					return Order.IBSOrderId.HasValue 
						? Order.IBSOrderId.Value.ToString(CultureInfo.InvariantCulture) 
						: "Error";
                }
                return null;
            }
		}

		public bool ShowConfirmationTxt
		{
			get
			{          
				return Settings.ShowOrderNumber 
					&& Order != null 
					&& Order.IBSOrderId.HasValue;
			}
		}

        private string _authorizationNumber;
        public string AuthorizationNumber 
		{
			get { return _authorizationNumber; }
            set
			{
                _authorizationNumber = value;
				RaisePropertyChanged ();
            }
        }

        public string RequestedTxt
        {
            get 
			{
                return Order != null 
					? FormatDateTime(Order.PickupDate, Order.PickupDate) 
					: null;
            }
        }

        public string OriginTxt
        {
            get 
			{
                return Order != null 
					? Order.PickupAddress.DisplayAddress 
					: null;
            }
        }

        public string AptRingTxt
        {
            get 
			{
                return Order != null 
                    ? FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode) 
                    : null;
            }
        }

        public string DestinationTxt
        {
            get
            {
                if (Order == null)
                {
                    return null;
                }
                return Order.DropOffAddress.FullAddress.HasValue()
                           ? Order.DropOffAddress.FullAddress
						   : this.Services().Localize["DestinationNotSpecifiedText"];
            }
        }

        public string PickUpDateTxt
        {
            get 
			{
                return Order != null 
                    ? FormatDateTime(Order.PickupDate, Order.PickupDate) 
                    : null;
            }
        }

		public string StatusDescription
		{
			get
			{
				if (Status.FareAvailable || Status.IsManualRideLinq)
				{
					var paymentAmount = Order.Fare.GetValueOrDefault()
					                   + Order.Tip.GetValueOrDefault()
					                   + Order.Tax.GetValueOrDefault()
					                   + Order.Toll.GetValueOrDefault()
					                   + Order.Surcharge.GetValueOrDefault();
				
					if (Status.FareAvailable)
					{
						return string.Format("{0} ({1})", Status.IBSStatusDescription, CultureProvider.FormatCurrency(paymentAmount));
					}
					else if (Status.IsManualRideLinq)
					{
						return string.Format("{0} ({1})", OrderStatus.Completed.ToString(), CultureProvider.FormatCurrency(paymentAmount));
					}
				}

				return Status.IBSStatusDescription;
			}
		}

		public string PromoCode
		{
			get 
			{
				return Order != null 
					? Order.PromoCode
					: null;
			}
		}

        public void RefreshOrderStatus (OrderRated orderRated)
		{
			if (orderRated.Content == OrderId) 
			{
				HasRated = true;
			}
        }

		public async Task LoadOrder() 
		{
			Order = await _accountService.GetHistoryOrderAsync(OrderId);
		}

		public async Task LoadStatus ()
		{
			var status = await _bookingService.GetOrderStatusAsync(OrderId);
			Status = status;

			var ratings = await _bookingService.GetOrderRatingAsync(OrderId);
			HasRated = ratings.RatingScores.Any();

			IsCompleted = _bookingService.IsStatusCompleted(Status);
			IsDone = _bookingService.IsStatusDone(Status.IBSStatusId);
            
			CanCancel = !IsDone && !IsCompleted && _bookingService.IsOrderCancellable (Status);
		}

		public ICommand NavigateToRatingPage
        {
            get
            {
            	return this.GetCommand(() =>
               	{
                    var canRate = IsDone && !HasRated;
					ShowSubViewModel<BookRatingViewModel,OrderRated>(new 
						{														
							orderId = OrderId.ToString(), 
							canRate
						},
						RefreshOrderStatus);
               	});
            }
        }

		public ICommand NavigateToOrderStatus
        {
            get
            {
                return this.GetCommand(() =>                                        
                {
					GoBackToOrder();
                });
            }
        }

		public ICommand DeleteOrder
        {
            get
            {
                return this.GetCommand(async () =>
                {
					using(this.Services().Message.ShowProgress())
					{
						if (OrderId.HasValue())
	                    {
							await _bookingService.RemoveFromHistory(OrderId);
	                        this.Services().MessengerHub.Publish(new OrderDeleted(this, OrderId, null));
							Close(this);
	                    }
					}
                });
            }
        }

		public void NavigateToHistoryList()
		{
			ShowViewModel<HistoryListViewModel>();
		}

		public ICommand RebookOrder
        {
            get
            {
                return this.GetCommand(async () =>
                {
					using(this.Services().Message.ShowProgress())
					{
						
							await _orderWorkflowService.Rebook(Order);
							GoBackToHomeViewModel(new { 
								locateUser =  false, 
								defaultHintZoomLevel = new ZoomToStreetLevelPresentationHint(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude).ToJson()});
					}
				});
            }
        }

		private async Task GoBackToOrder ()
		{
			if (Order != null)
			{
				if (Order.IsManualRideLinq)
				{
					var orderManualRideLinqDetail = await Task.Run (() => _bookingService.GetTripInfoFromManualRideLinq (Order.Id));

					ShowViewModel<HomeViewModel> (new
	                {
	                    manualRidelinqDetail = orderManualRideLinqDetail.Data.ToJson (),
	                    locateUser = false
	                });

					return;
				}

				ShowViewModel<HomeViewModel> (new
	            {
	                order = Order.ToJson (),
	                orderStatusDetail = Status.ToJson (),
	                locateUser = false
	            });
			}
		}

		public ICommand SendReceipt
        {
            get
            {
                return this.GetCommand(async () =>
                {
					using(this.Services().Message.ShowProgress())
					{
						if (OrderId.HasValue())
						{
							await _bookingService.SendReceipt(OrderId);
						}
						Close(this);
					}
                });
            }
        }

		public ICommand CancelOrder
        {
            get
            {
				var statusConfirmCancelRideAndWarnForCancellationFees = string.Format(this.Services().Localize["StatusConfirmCancelRideAndWarnForCancellationFees"], Settings.TaxiHail.ApplicationName);
				var statusConfirmCancelRide = this.Services().Localize["StatusConfirmCancelRide"];

                return this.GetCommand(() => this.Services().Message.ShowMessage(
					string.Empty,
					Settings.WarnForFeesOnCancel && VehicleStatuses.CanCancelOrderStatusButCouldBeChargedFees.Contains(Status.IBSStatusId) 
						? statusConfirmCancelRideAndWarnForCancellationFees 
						: statusConfirmCancelRide, 
                    this.Services().Localize["YesButton"], 
					async () =>
	                	{
							using(this.Services().Message.ShowProgress())
							{
								var isSuccess = await _bookingService.CancelOrder(OrderId);
			                    if(isSuccess)
			                    {
                                    this.Services().MessengerHub.Publish(new OrderStatusChanged(this, OrderId, OrderStatus.Canceled, null));
			                        LoadStatus();
			                    }
			                    else
			                    {
			                        InvokeOnMainThread(() => this.Services().Message.ShowMessage(
										this.Services().Localize["StatusConfirmCancelRideErrorTitle"], 
		                                this.Services().Localize["StatusConfirmCancelRideError"]));
			                    }
							}
		                },
                    this.Services().Localize["NoButton"], () => { })); 
            }
        }

        private string FormatDateTime(DateTime? date, DateTime? time)
        {
            var result = date.HasValue 
				? date.Value.ToShortDateString() 
				: this.Services().Localize["DateToday"];
            result += @" / ";
            result += time.HasValue 
				? time.Value.ToShortTimeString() 
				: this.Services().Localize["TimeNow"];
            return result;
        }

        private string FormatAptRingCode(string apt, string rCode)
        {
			var result = apt.HasValue() 
				? apt 
				: this.Services().Localize["NoAptText"];
            result += @" / ";
			result += rCode.HasValue() 
				? rCode 
				: this.Services().Localize["NoRingCodeText"];
            return result;
        }
    }
}