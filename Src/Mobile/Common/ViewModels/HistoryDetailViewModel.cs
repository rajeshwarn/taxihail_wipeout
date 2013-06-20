using System;
using System.Linq;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using ServiceStack.Text;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryDetailViewModel : BaseViewModel
    {
        private Guid _orderId;
        public Guid OrderId
        {
            get
            {
                return _orderId;
            }
            set { _orderId = value; FirePropertyChanged("OrderId"); }

        }

		private Order _order;
		public Order Order {
			get{ return _order; }
            set { _order = value; FirePropertyChanged("Order"); 
                FirePropertyChanged("ConfirmationTxt"); 
                FirePropertyChanged("RequestedTxt"); 
                FirePropertyChanged("OriginTxt"); 
                FirePropertyChanged("AptRingTxt"); 
                FirePropertyChanged("DestinationTxt"); 
                FirePropertyChanged("PickUpDateTxt"); }
		}

        private OrderStatusDetail _status = new OrderStatusDetail{ IBSStatusDescription = TinyIoCContainer.Current.Resolve<IAppResource>().GetString( "LoadingMessage") };
		public OrderStatusDetail Status {
			get{ return _status; }
            set { _status = value; FirePropertyChanged("Status");
            FirePropertyChanged("SendReceiptAvailable");
            
            }


		}

        

        public bool SendReceiptAvailable 
        {
            get
            {
               var sendReceiptAvailable = !TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.SendReceiptAvailable").TryToParse( false);
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
            set { _isDone = value; FirePropertyChanged("IsDone"); FirePropertyChanged("ShowRateButton"); }

        }

        private bool _isCompleted = true;
		public bool IsCompleted {
			get {
				return _isCompleted;
			}
			set { 
				if (value != _isCompleted) {
					_isCompleted = value;
					FirePropertyChanged ("IsCompleted");
				}
			}
			
		}

        private bool _hasRated;
        public bool HasRated
        {
            get
            {
                if (!TinyIoCContainer.Current.Resolve<IAppSettings>().RatingEnabled)
                {
                    return false;
                }
                else
                {
                    return _hasRated;
                }
                
            }
            set { _hasRated = value; FirePropertyChanged("HasRated"); FirePropertyChanged("ShowRateButton"); }

        }

        private bool _showRateButton;
        public bool ShowRateButton
        {
            get
            {
                if (!TinyIoCContainer.Current.Resolve<IAppSettings>().RatingEnabled)
                {
                    return false;
                }
                else
                {
                    return IsDone && !HasRated;
                }
            }
            set { _showRateButton = value; FirePropertyChanged("ShowRateButton"); }

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
                    FirePropertyChanged("CanCancel");
                }
            }
        }

        public string ConfirmationTxt
        {
            get
            {
                if (Order != null)
                {
                    return Order.IBSOrderId.HasValue ? Order.IBSOrderId.Value.ToString() : "Error";
                    
                }
                else
                {
                    return null;
                }
            }
        }

        public string RequestedTxt
        {
            get
            {
                if (Order != null)
                {
                    return FormatDateTime(Order.PickupDate, Order.PickupDate);
                }
                else
                {
                    return null;
                }
            }
        }

        public string OriginTxt
        {
            get
            {
                if (Order != null)
                {
                    return Order.PickupAddress.BookAddress;
                }
                else
                {
                    return null;
                }
            }
        }

        public string AptRingTxt
        {
            get
            {
                if (Order != null)
                {
                    return FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode);
                }
                else
                {
                    return null;
                }
            }
        }

        public string DestinationTxt
        {
            get
            {
                if (Order != null)
                {
                    var resources = TinyIoCContainer.Current.Resolve<IAppResource>();
                    return Order.DropOffAddress.FullAddress.HasValue()
                               ? Order.DropOffAddress.FullAddress
                               : resources.GetString("ConfirmDestinationNotSpecified");
                }
                else
                {
                    return null;
                }
            }
        }

        public string PickUpDateTxt
        {
            get
            {
                if (Order != null)
                {
                    return FormatDateTime(Order.PickupDate, Order.PickupDate);
                }
                else
                {
                    return null;
                }
            }
        }

        public HistoryDetailViewModel(string orderId)
        {
            
			Guid id;
            if(Guid.TryParse(orderId, out id)) {
				OrderId = id;
			}
        }

		protected override void Initialize ()
		{
			MessengerHub.Subscribe<OrderRated>(RefreshOrderStatus);
		}

		public override void Load ()
        {
			base.Load ();
            LoadOrder();
            LoadStatus();
        }

        public void RefreshOrderStatus (OrderRated orderRated)
		{
			if (orderRated.Content == this.OrderId) {
				LoadStatus();
			}
        }

		public Task LoadOrder() 
		{
			return Task.Factory.StartNew(() => {
				this.Order = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrder(this.OrderId);
			});

		}

		public Task LoadStatus ()
		{
			return Task.Factory.StartNew(()=> {
                var bookingService = TinyIoCContainer.Current.Resolve<IBookingService>();

                HasRated = bookingService.GetOrderRating(OrderId).RatingScores.Any();
                Status = bookingService.GetOrderStatus(OrderId);
				IsCompleted = TinyIoCContainer.Current.Resolve<IBookingService> ().IsStatusCompleted (Status.IBSStatusId);
                IsDone = bookingService.IsStatusDone(Status.IBSStatusId);
                var isCompleted = bookingService.IsStatusCompleted(Status.IBSStatusId);

			    CanCancel = !isCompleted;
			});

		}

        public IMvxCommand NavigateToRatingPage
        {
            get
            {
                return GetCommand(() =>
                                               {
                                                   var canRate = IsDone && !HasRated;
					RequestNavigate<BookRatingViewModel>(new { orderId = OrderId, canRate = canRate.ToString()});
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
						IBSOrderId = Order.IBSOrderId,
						IBSStatusDescription = "Loading...",
						IBSStatusId = "",
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
                    if (Common.Extensions.GuidExtensions.HasValue(OrderId))
                    {
                        TinyIoCContainer.Current.Resolve<IBookingService>().RemoveFromHistory(OrderId);
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
                    if (Common.Extensions.GuidExtensions.HasValue(OrderId))
                    {
                        TinyIoCContainer.Current.Resolve<IBookingService>().SendReceipt(OrderId);
                    }
                    RequestClose(this);
                });
            }
        }

         public IMvxCommand CancelOrder
        {
            get
            {
                return GetCommand(() =>
                {
                    var messageService = TinyIoCContainer.Current.Resolve<IMessageService>();
                    var resources = TinyIoCContainer.Current.Resolve<IAppResource>();
                    messageService.ShowMessage(string.Empty, resources.GetString("StatusConfirmCancelRide"), resources.GetString("YesButton"), () =>
                    {
                        var bookingService = TinyIoCContainer.Current.Resolve<IBookingService>();
                        var isSuccess = bookingService.CancelOrder(OrderId);

                        if(isSuccess)
                        {
                            LoadStatus();
                        }
                        else
                        {
                            InvokeOnMainThread(() => messageService.ShowMessage(resources.GetString("StatusConfirmCancelRideErrorTitle"), resources.GetString("StatusConfirmCancelRideError")));
                        }
                    },
                    resources.GetString("NoButton"), () => { });                          
                }); 
            }
        }

        private string FormatDateTime(DateTime? date, DateTime? time)
        {
            var resources = TinyIoCContainer.Current.Resolve<IAppResource>();
            string result = date.HasValue ? date.Value.ToShortDateString() :  resources.GetString("DateToday");
            result += @" / ";
            result += time.HasValue ? time.Value.ToShortTimeString() : resources.GetString("TimeNow");
            return result;
        }

        private string FormatAptRingCode(string apt, string rCode)
        {
            var resources = TinyIoCContainer.Current.Resolve<IAppResource>();
            string result = apt.HasValue() ? apt : resources.GetString("ConfirmNoApt");

            result += @" / ";
            result += rCode.HasValue() ? rCode : resources.GetString("ConfirmNoRingCode");
            return result;
        }
    }
}