using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Commands;
using ServiceStack.Text;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookingStatusViewModel : BaseViewModel
    {

        private Order _order;

        public Order Order
        {
            get { return _order; }
            set
            {
                _order = value;
                FirePropertyChanged(() => Order);
            }
        }

        private OrderStatusDetail _orderStatusDetail;

        public OrderStatusDetail OrderStatusDetail
        {
            get { return _orderStatusDetail; }
            set
            {
                _orderStatusDetail = value;
                FirePropertyChanged(() => OrderStatusDetail);
            }
        }

		public bool CloseScreenWhenCompleted {
			get;
			set;
		}

        private bool _showRatingButton;

        public bool ShowRatingButton
        {
            get
            {
                if (!TinyIoCContainer.Current.Resolve<IAppSettings>().RatingEnabled)
                {
                    return false;
                }
                else
                {
                    return _showRatingButton;
                }
            }
            set 
            { 
                _showRatingButton = value;
                FirePropertyChanged(()=>ShowRatingButton);
            }
        }

        public BookingStatusViewModel()
        {
            ShowRatingButton = true;
        }

        public BookingStatusViewModel(string order)
        {
            OrderWithStatusModel orderWithStatus = JsonSerializer.DeserializeFromString < OrderWithStatusModel>(order);
            Order = orderWithStatus.Order;
            OrderStatusDetail = orderWithStatus.OrderStatusDetail;
            ShowRatingButton = true;
			//CloseScreenWhenCompleted = bool.Parse(closeScreenWhenCompleted);
        }

        public MvxRelayCommand NavigateToRatingPage
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<OrderRated>(HideRatingButton);
                    RequestNavigate<BookRatingViewModel>(new { orderId = Order.Id.ToString(), canRate = "true" });
                });
            }
        }

        private void HideRatingButton(OrderRated orderRated)
        {
            ShowRatingButton = false;
        }
    }
}