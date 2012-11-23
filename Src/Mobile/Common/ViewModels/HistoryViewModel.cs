using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using ServiceStack.Text;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Extensions;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        //TODO : a remplacer quand les strings seront globalisee
        private const string titleFormat = "Order #{0} ({1:ddd, MMM d}, {1:h:mm tt})";
        private ObservableCollection<OrderViewModel> _orders;

        public ObservableCollection<OrderViewModel> Orders
        {
            get { return _orders; }
            set { _orders = value; FirePropertyChanged("Orders"); }
        }

        private bool _hasOrders;
        public bool HasOrders {
            get {
                return _hasOrders;
            }
            set {
                if(value != _hasOrders) {
                    _hasOrders = value;
                    FirePropertyChanged("HasOrders");
                }
            }
        }

        public HistoryViewModel()
        {
			LoadOrders ();
        }

		public Task LoadOrders ()
		{
			return Task.Factory.StartNew (() => {
				var orders = TinyIoCContainer.Current.Resolve<IAccountService> ().GetHistoryOrders ().ToArray();
				Orders = new ObservableCollection<OrderViewModel> (orders.Select (x => new OrderViewModel ()
					{
						IBSOrderId = x.IBSOrderId,
						Id = x.Id,
						CreatedDate = x.CreatedDate,
						PickupAddress = x.PickupAddress,
						PickupDate = x.PickupDate, 
						IsCompleted = x.IsCompleted,
						Title = String.Format(titleFormat, x.IBSOrderId.ToString(), x.PickupDate),
						IsFirst = x.Equals(orders.First()),
						IsLast = x.Equals(orders.Last()),
						ShowRightArrow = true
					}));

                HasOrders = orders.Any();
			});
		}


        public IMvxCommand NavigateToHistoryDetailPage
        {
            get
            {
                return new MvxRelayCommand<OrderViewModel>(vm => RequestNavigate<HistoryDetailViewModel>(
                    new {orderId = vm.Id}));
            }
        }
    }
}