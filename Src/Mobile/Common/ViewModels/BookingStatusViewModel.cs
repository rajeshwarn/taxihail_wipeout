using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Commands;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
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

        public BookingStatusViewModel()
        {
            
        }

        public BookingStatusViewModel(string order)
        {
            OrderWithStatusModel orderWithStatus = JsonSerializer.DeserializeFromString < OrderWithStatusModel>(order);
            Order = orderWithStatus.Order;
            OrderStatusDetail = orderWithStatus.OrderStatusDetail;
        }

        public MvxRelayCommand NavigateToRatingPage
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    RequestNavigate<BookRatingViewModel>(new { orderId = Order.Id.ToString(), canRate = "true" });
                });
            }
        }
    }
}