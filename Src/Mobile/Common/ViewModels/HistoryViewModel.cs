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
using Cirrious.MvvmCross.Interfaces.Commands;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        //TODO : a remplacer quand les strings seront globalisee
        private const string titleFormat = "Order #{0} ({1:ddd, MMM d}, {1:h:mm tt})";
        private List<OrderViewModel> _orders;

        public List<OrderViewModel> Orders
        {
            get { return _orders; }
            set { _orders = value; FirePropertyChanged("Orders"); }
        }
        public HistoryViewModel()
        {
            IEnumerable<Order> orders = new Order[0];
            orders = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrders().ToList();
            Orders = orders.Select(x =>
                                           new OrderViewModel()
                                               {
                                                   IBSOrderId = x.IBSOrderId,
                                                   Id = x.Id,
                                                   CreatedDate = x.CreatedDate,
                                                   PickupAddress = x.PickupAddress,
                                                   IsCompleted = x.IsCompleted,
                                                   Title = string.Format(titleFormat,x.IBSOrderId.ToString(), x.CreatedDate),
                                                   IsFirst = x.Equals(orders.First()),
                                                   IsLast = x.Equals(orders.Last()),
                                                   ShowRightArrow = true
                                               }
                                       ).ToList();
            Orders.ForEach(c=>
                               {
                                   c.OrderRatings =
                                       TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderRating(c.Id);
                               });
        }


        public IMvxCommand NavigateToHistoryDetailPage
        {
            get
            {
                return new MvxRelayCommand<OrderViewModel>(vm => 
                {
                    
                    var not = !vm.IsCompleted;
                    if(vm.OrderRatings.RatingScores.Any())
                    {
                        not = false;
                    }
                    RequestNavigate<HistoryDetailViewModel>(new { orderId = vm.Id, canRate = not.ToString()});
                });
            }
        }
    }
}