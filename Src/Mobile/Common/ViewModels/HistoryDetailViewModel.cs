using System;
using System.Globalization;
using System.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;
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
        public HistoryDetailViewModel()
        {
            _status = new OrderStatusDetail
            {
                IbsStatusDescription = Resources.GetString("LoadingMessage")
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
				FirePropertyChanged(()=>OrderId); 
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
                FirePropertyChanged(()=>Order); 
                FirePropertyChanged(()=>ConfirmationTxt); 
                FirePropertyChanged(()=>RequestedTxt); 
                FirePropertyChanged(()=>OriginTxt); 
                FirePropertyChanged(()=>AptRingTxt); 
                FirePropertyChanged(()=>DestinationTxt); 
                FirePropertyChanged(()=>PickUpDateTxt); 
            }
		}

        private OrderStatusDetail _status;

		public OrderStatusDetail Status 
        {
			get{ return _status; }
		    set
		    {
		        _status = value;
		        FirePropertyChanged("Status");
		        FirePropertyChanged("SendReceiptAvailable");
		    }
		}
        
        
        public bool SendReceiptAvailable 
        {
            get
            {
                var sendReceiptAvailable =  ConfigurationManager.GetSetting("Client.SendReceiptAvailable",false);
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
				FirePropertyChanged(()=>IsDone); 
				FirePropertyChanged(()=>ShowRateButton); 
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
					FirePropertyChanged (()=>IsCompleted);
					FirePropertyChanged (()=>RebookIsAvailable);
				}
			}
		}

		public bool RebookIsAvailable {
			get {
				var setting = ConfigurationManager.GetSetting("Client.HideRebookOrder");
				return IsCompleted && !bool.Parse(string.IsNullOrWhiteSpace(setting) ? bool.FalseString : setting);
			}
		}

        private bool _hasRated;
        public bool HasRated
        {
            get {
                var ratingEnabled = Config.GetSetting( "Client.RatingEnabled", false );  
                return ratingEnabled && _hasRated;
            }
            set { 
				_hasRated = value; 
				FirePropertyChanged(()=>HasRated); 
				FirePropertyChanged(()=>ShowRateButton);  
			}
        }

        public bool ShowRateButton
        {
            get
            {
                    var ratingEnabled = Config.GetSetting( "Client.RatingEnabled", false );                
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
                    FirePropertyChanged(()=>CanCancel);
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
                FirePropertyChanged (()=>AuthorizationNumber);
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
				return Config.GetSetting("Client.HideDestination", false);
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
                           : Resources.GetString("ConfirmDestinationNotSpecified");
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

        public HistoryDetailViewModel(string orderId)
        {
			Guid id;
            if(Guid.TryParse(orderId, out id)) {
				OrderId = id;
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
				Order = AccountService.GetHistoryOrder(OrderId);
			});
		}

		public event EventHandler Loaded;

		public Task LoadStatus ()
		{
			return Task.Factory.StartNew(()=> {
                HasRated = BookingService.GetOrderRating(OrderId).RatingScores.Any();
                Status = BookingService.GetOrderStatus(OrderId);
				IsCompleted = BookingService.IsStatusCompleted (Status.IbsStatusId);
				IsDone = BookingService.IsStatusDone(Status.IbsStatusId);
                
				CanCancel = !IsCompleted;

				if(Loaded!=null){
					Loaded(this, null);
				}
			});
		}

        public IMvxCommand NavigateToRatingPage
        {
            get
            {
                return GetCommand(() =>
                                               {
                                                   var canRate = IsDone && !HasRated;
													RequestSubNavigate<BookRatingViewModel,OrderRated>(new 
					            	                    {														
															orderId = OrderId, 
															canRate = canRate.ToString()
														}.ToStringDictionary(),_=>
														{
														RefreshOrderStatus(_);
														});
                                               });
            }
        }

        public IMvxCommand NavigateToOrderStatus
        {
            get
            {
                return GetCommand(() =>                                        
                {
					var orderStatus = new OrderStatusDetail
					{ 
							IbsOrderId = Order.IbsOrderId,
							IbsStatusDescription = "Loading...",
							IbsStatusId = "",
							OrderId = OrderId,
							Status = OrderStatus.Unknown,
							VehicleLatitude = null,
							VehicleLongitude = null
					};
					RequestNavigate<BookingStatusViewModel>(new {
						order =  Order.ToJson(),
						orderStatus = orderStatus.ToJson()
					});
                });
            }
        }

        public IMvxCommand DeleteOrder
        {
            get
            {
                return GetCommand(() =>
                {
                    if (GuidExtensions.HasValue(OrderId))
                    {
                        BookingService.RemoveFromHistory(OrderId);
                        MessengerHub.Publish(new OrderDeleted(this,OrderId,null));
                        Close();
                    }
                });
            }
        }

        public IMvxCommand RebookOrder
        {
            get
            {
                return GetCommand(() =>
                {
                    var serialized = JsonSerializer.SerializeToString(Order);
                    RequestNavigate<BookViewModel>(new { order = serialized }, clearTop: true, requestedBy: MvxRequestedBy.UserAction);
                });
            }
        }

        public IMvxCommand SendReceipt
        {
            get
            {
                return GetCommand(() =>
                {
                    if (GuidExtensions.HasValue(OrderId))
                    {
                        BookingService.SendReceipt(OrderId);
                    }
                    RequestClose(this);
                });
            }
        }

         public IMvxCommand CancelOrder
        {
            get
            {
                return GetCommand(() => MessageService.ShowMessage(string.Empty, Resources.GetString("StatusConfirmCancelRide"), Resources.GetString("YesButton"), () =>
                {
                        
                    var isSuccess = BookingService.CancelOrder(OrderId);

                    if(isSuccess)
                    {
                        LoadStatus();
                    }
                    else
                    {
                        InvokeOnMainThread(() => MessageService.ShowMessage(Resources.GetString("StatusConfirmCancelRideErrorTitle"), Resources.GetString("StatusConfirmCancelRideError")));
                    }
                },
                    Resources.GetString("NoButton"), () => { })); 
            }
        }

        private string FormatDateTime(DateTime? date, DateTime? time)
        {
            var result = date.HasValue ? date.Value.ToShortDateString() : Resources.GetString("DateToday");
            result += @" / ";
            result += time.HasValue ? time.Value.ToShortTimeString() : Resources.GetString("TimeNow");
            return result;
        }

        private string FormatAptRingCode(string apt, string rCode)
        {
            var result = apt.HasValue() ? apt : Resources.GetString("ConfirmNoApt");

            result += @" / ";
            result += rCode.HasValue() ? rCode : Resources.GetString("ConfirmNoRingCode");
            return result;
        }
    }
}