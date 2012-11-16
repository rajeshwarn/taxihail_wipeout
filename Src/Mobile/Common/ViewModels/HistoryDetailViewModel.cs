using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.ViewModels;
using ServiceStack.Text;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryDetailViewModel : BaseViewModel
    {
		public event EventHandler Deleted;
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
			set{ _order = value; FirePropertyChanged("Order"); }
		}

		private OrderStatusDetail _status;
		public OrderStatusDetail Status {
			get{ return _status; }
			set{ _status = value; FirePropertyChanged("Status"); }
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

		private bool _isCompleted;
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

        public HistoryDetailViewModel(string orderId)
        {
			Guid id;
            if(Guid.TryParse(orderId, out id)) {
				OrderId = id;
			}
        }

		public void Initialize ()
		{
			LoadOrder();
			LoadStatus();
			TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<OrderRated>(RefreshOrderStatus);
			
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
				HasRated = TinyIoCContainer.Current.Resolve<IBookingService> ().GetOrderRating (OrderId).RatingScores.Any ();
				Status = TinyIoCContainer.Current.Resolve<IBookingService> ().GetOrderStatus (OrderId);
				IsDone = TinyIoCContainer.Current.Resolve<IBookingService> ().IsStatusDone (Status.IBSStatusId);
				IsCompleted = TinyIoCContainer.Current.Resolve<IBookingService> ().IsStatusCompleted (Status.IBSStatusId);
			});

		}

        public MvxRelayCommand NavigateToRatingPage
        {
            get
            {
                return new MvxRelayCommand(() =>
                                               {
                                                   var canRate = IsDone && !HasRated;
                                                   RequestNavigate<BookRatingViewModel>(new { orderId = OrderId, canRate = canRate.ToString() });
                                               });
            }
        }

        public IMvxCommand NavigateToOrderStatus
        {
            get
            {
                return new MvxRelayCommand<Dictionary<string,object>>(order =>
                                                                  {
                                                                      var orderGet = (Order) order["order"];
                                                                      var orderInfoGet = (OrderStatusDetail) order["orderInfo"];
                                                                      var orderWithStatus = new OrderWithStatusModel() { Order = orderGet, OrderStatusDetail = orderInfoGet };
                                                                      var serialized = JsonSerializer.SerializeToString(orderWithStatus, typeof(OrderWithStatusModel));
                                                                      RequestNavigate<BookingStatusViewModel>(new {order = serialized});
                });
            }
        }

        public IMvxCommand DeleteOrder
        {
            get
            {
                return new MvxRelayCommand<Guid>(orderId =>
                {
                    if (Common.Extensions.GuidExtensions.HasValue(orderId))
                        {
                            TinyIoCContainer.Current.Resolve<IBookingService>().RemoveFromHistory(orderId);
						    if(Deleted != null)
							{
								Deleted(this, EventArgs.Empty);
							}
						    RequestClose(this);
                        }
                });
            }
        }
    }
}