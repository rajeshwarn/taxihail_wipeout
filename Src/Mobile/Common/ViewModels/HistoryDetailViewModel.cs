using System;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Messages;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryDetailViewModel : BaseViewModel
    {
		IOrderWorkflowService _orderWorkflowService;

		public HistoryDetailViewModel(IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;			
		}

		public void Init(string orderId)
		{
			Guid id;
			if(Guid.TryParse(orderId, out id))
			{
				OrderId = id;
			}
		}

		public override void Start()
		{
			base.Start();
			_status = new OrderStatusDetail
			{
				IBSStatusDescription = this.Services().Localize["LoadingMessage"]
			};
		}

        private Guid _orderId;
        public Guid OrderId
        {
            get
            {
                return _orderId;
            }
            set { 
				_orderId = value; 
				RaisePropertyChanged(); 
			}
        }

		private Order _order;
		public Order Order {
			get{ return _order; }
            set { 
				_order = value; 
                if (_order.TransactionId != default(long)) {
                    AuthorizationNumber = _order.TransactionId + "";
                }
				RaisePropertyChanged(()=>Order); 
				RaisePropertyChanged(()=>ConfirmationTxt); 
				RaisePropertyChanged(()=>RequestedTxt); 
				RaisePropertyChanged(()=>OriginTxt); 
				RaisePropertyChanged(()=>AptRingTxt); 
				RaisePropertyChanged(()=>DestinationTxt); 
				RaisePropertyChanged(()=>PickUpDateTxt); 
            }
		}

        private OrderStatusDetail _status;
		public OrderStatusDetail Status 
        {
			get{ return _status; }
		    set
		    {
		        _status = value;
				RaisePropertyChanged();
				RaisePropertyChanged("SendReceiptAvailable");
		    }
		}
        
        public bool SendReceiptAvailable 
        {
            get
            {
				return (Status != null) 
						&& Status.FareAvailable 
						&& Settings.SendReceiptAvailable;
            }
        }

        private bool _isDone;
        public bool IsDone
        {
            get
            {
                return _isDone;
            }
            set { 
				_isDone = value; 
				RaisePropertyChanged(); 
				RaisePropertyChanged(()=>ShowRateButton); 
			}
        }

        private bool _isCompleted = true;
		public bool IsCompleted {
			get {
				return _isCompleted;
			}
			set { 
				if (value != _isCompleted) {
					_isCompleted = value;
					RaisePropertyChanged ();
					RaisePropertyChanged (()=>RebookIsAvailable);
				}
			}
		}

		public bool RebookIsAvailable {
			get {
                
				return IsCompleted 
						&& !Settings.HideRebookOrder;
			}
		}

        private bool _hasRated;
        public bool HasRated
        {
            get {
				return !Settings.RatingEnabled && _hasRated;
            }
            set { 
				_hasRated = value; 
				RaisePropertyChanged(); 
				RaisePropertyChanged(()=>ShowRateButton);  
			}
        }

        public bool ShowRateButton
        {
            get
            {          
				return Settings.RatingEnabled && IsDone && !HasRated;
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
					return Order.IBSOrderId.HasValue ? Order.IBSOrderId.Value.ToString(CultureInfo.InvariantCulture) : "Error";
                }
                return null;
            }
        }

        private string _authorizationNumber;
        public string AuthorizationNumber {
            get {
                return _authorizationNumber;
            }
            set{
                _authorizationNumber = value;
				RaisePropertyChanged ();
            }
        }

        public string RequestedTxt
        {
            get {
                return Order != null ? FormatDateTime(Order.PickupDate, Order.PickupDate) : null;
            }
        }

        public string OriginTxt
        {
            get {
                return Order != null ? Order.PickupAddress.DisplayAddress : null;
            }
        }

        public string AptRingTxt
        {
            get {
                return Order != null 
                    ? 
                    FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode) 
                    : null;
            }
        }

		public bool HideDestination
		{
			get
			{
				return !Settings.HideDestination;
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
            get {
                return Order != null 
                    ? FormatDateTime(Order.PickupDate, Order.PickupDate) 
                    : null;
            }
        }

		public override async void OnViewLoaded ()
        {
			base.OnViewLoaded ();
			LoadOrder();
            LoadStatus();
        }

        public void RefreshOrderStatus (OrderRated orderRated)
		{
			if (orderRated.Content == OrderId) {
				HasRated = true;
			}
        }

		public async void LoadOrder() 
		{
			Order = await this.Services().Account.GetHistoryOrderAsync(OrderId);
		}

		public async void LoadStatus ()
		{
			var bookingService = this.Services().Booking;

			var ratings = bookingService.GetOrderRatingAsync(OrderId);
			var status = bookingService.GetOrderStatusAsync(OrderId);

			HasRated = (await ratings).RatingScores.Any();
			Status = await status;
			IsCompleted = bookingService.IsStatusCompleted(Status.IBSStatusId);
			IsDone = bookingService.IsStatusDone(Status.IBSStatusId);
            
			CanCancel = !IsCompleted;
		}

        public AsyncCommand NavigateToRatingPage
        {
            get
            {
                return GetCommand(() =>
                                               {
                                                   var canRate = IsDone && !HasRated;
													ShowSubViewModel<BookRatingViewModel,OrderRated>(new 
					            	                    {														
															orderId = OrderId, 
															canRate = canRate
														}.ToStringDictionary(),
													RefreshOrderStatus);
                                               });
            }
        }

        public AsyncCommand NavigateToOrderStatus
        {
            get
            {
                return GetCommand(() =>                                        
                {
					var orderStatus = new OrderStatusDetail
					{ 
							IBSOrderId = Order.IBSOrderId,
							IBSStatusDescription = this.Services().Localize["LoadingMessage"],
							IBSStatusId = "",
							OrderId = OrderId,
							Status = OrderStatus.Unknown,
							VehicleLatitude = null,
							VehicleLongitude = null
					};
					ShowViewModel<BookingStatusViewModel>(new {
						order =  Order.ToJson(),
						orderStatus = orderStatus.ToJson()
					});
                });
            }
        }

        public AsyncCommand DeleteOrder
        {
            get
            {
                return GetCommand(() =>
                {
                    if (GuidExtensions.HasValue(OrderId))
                    {
                        this.Services().Booking.RemoveFromHistory(OrderId);
                        this.Services().MessengerHub.Publish(new OrderDeleted(this, OrderId, null));
						Close(this);
                    }
                });
            }
        }

        public AsyncCommand RebookOrder
        {
            get
            {
                return GetCommand(() =>
                {
					_orderWorkflowService.Rebook(Order);
					ShowViewModel<HomeViewModel>();
                });
            }
        }

        public AsyncCommand SendReceipt
        {
            get
            {
                return GetCommand(() =>
                {
                    if (OrderId.HasValue())
                    {
                        this.Services().Booking.SendReceipt(OrderId);
                    }
                    Close(this);
                });
            }
        }

        public AsyncCommand CancelOrder
        {
            get
            {
                return GetCommand(() => this.Services().Message.ShowMessage(string.Empty, this.Services().Localize["StatusConfirmCancelRide"], 
                                                                                                   this.Services().Localize["YesButton"], () =>
                {

                    var isSuccess = this.Services().Booking.CancelOrder(OrderId);

                    if(isSuccess)
                    {
                        LoadStatus();
                    }
                    else
                    {
                        InvokeOnMainThread(() => this.Services().Message.ShowMessage(this.Services().Localize["StatusConfirmCancelRideErrorTitle"], 
                                                                                            this.Services().Localize["StatusConfirmCancelRideError"]));
                    }
                },
                    this.Services().Localize["NoButton"], () => { })); 
            }
        }

        private string FormatDateTime(DateTime? date, DateTime? time)
        {
            var result = date.HasValue ? date.Value.ToShortDateString() : this.Services().Localize["DateToday"];
            result += @" / ";
            result += time.HasValue ? time.Value.ToShortTimeString() : this.Services().Localize["TimeNow"];
            return result;
        }

        private string FormatAptRingCode(string apt, string rCode)
        {
			var result = apt.HasValue() ? apt : this.Services().Localize["NoAptText"];

            result += @" / ";
			result += rCode.HasValue() ? rCode : this.Services().Localize["NoRingCodeText"];
            return result;
        }
    }
}