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


namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryDetailViewModel : BaseViewModel
    {
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
				IbsStatusDescription = this.Services().Localize["LoadingText"]
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
                var sendReceiptAvailable = this.Services().Config.GetSetting("Client.SendReceiptAvailable", false);
                return (Status != null) && Status.FareAvailable && sendReceiptAvailable;
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
                var setting = this.Services().Config.GetSetting("Client.HideRebookOrder");
				return IsCompleted && !bool.Parse(string.IsNullOrWhiteSpace(setting) ? bool.FalseString : setting);
			}
		}

        private bool _hasRated;
        public bool HasRated
        {
            get {
                var ratingEnabled = this.Services().Config.GetSetting("Client.RatingEnabled", false);  
                return ratingEnabled && _hasRated;
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
                var ratingEnabled = this.Services().Config.GetSetting("Client.RatingEnabled", false);                
                    return ratingEnabled && IsDone && !HasRated;
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
					return Order.IbsOrderId.HasValue ? Order.IbsOrderId.Value.ToString(CultureInfo.InvariantCulture) : "Error";
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
                return this.Services().Config.GetSetting("Client.HideDestination", false);
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

		public override void Load ()
        {
			base.Load ();
            LoadOrder();
            LoadStatus();
        }

        public void RefreshOrderStatus (OrderRated orderRated)
		{
			if (orderRated.Content == OrderId) {
				LoadStatus();
			}
        }

		public Task LoadOrder() 
		{
			return Task.Factory.StartNew(() => {
                Order = this.Services().Account.GetHistoryOrder(OrderId);
			});
		}

		public event EventHandler Loaded;

		public Task LoadStatus ()
		{
			return Task.Factory.StartNew(()=> {
                HasRated = this.Services().Booking.GetOrderRating(OrderId).RatingScores.Any();
                Status = this.Services().Booking.GetOrderStatus(OrderId);
                IsCompleted = this.Services().Booking.IsStatusCompleted(Status.IbsStatusId);
                IsDone = this.Services().Booking.IsStatusDone(Status.IbsStatusId);
                
				CanCancel = !IsCompleted;

				if(Loaded!=null){
					Loaded(this, null);
				}
			});
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
														}.ToStringDictionary(),RefreshOrderStatus);
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
							IbsOrderId = Order.IbsOrderId,
							IbsStatusDescription = this.Services().Localize["LoadingText"],
							IbsStatusId = "",
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
                    var serialized = JsonSerializer.SerializeToString(Order);
                    ShowViewModel<BookViewModel>(new { order = serialized });
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