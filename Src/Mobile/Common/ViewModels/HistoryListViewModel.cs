using System;
using System.Collections.ObjectModel;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HistoryListViewModel : PageViewModel
    {
		private readonly IAccountService _accountService;

		public HistoryListViewModel(IAccountService accountService)
        {
			_accountService = accountService;

			_orderDeletedToken = this.Services().MessengerHub.Subscribe<OrderDeleted>(c => OnOrderDeleted(c.Content));  
        }

		private ObservableCollection<OrderViewModel> _orders;
		private readonly TinyMessageSubscriptionToken _orderDeletedToken;

		public void Init()
		{
			HasOrders = true; //Needs to be true otherwise we see the no order for a few seconds 
		}

        public ObservableCollection<OrderViewModel> Orders
        {
            get { return _orders; }
			set { _orders = value; RaisePropertyChanged(); }
        }

        private string FormatDateTime(DateTime date )
        {
            var formatTime = new CultureInfo(CultureProvider.CultureInfoString).DateTimeFormat.ShortTimePattern;
            string format = "{0:dddd, MMMM d}, {0:"+formatTime+"}";
            string result = string.Format(format, date) ;
            return result;
        }
       
        private bool _hasOrders;
        public bool HasOrders {
            get {
                return _hasOrders;
            }
            set {
                if(value != _hasOrders) {
                    _hasOrders = value;
					RaisePropertyChanged();
                }
            }
        }

        private void OnOrderDeleted(Guid orderId)
        {
            Task.Factory.StartNew(() =>
            {
                Orders = new ObservableCollection<OrderViewModel>(Orders.Where(order=>!order.Id.Equals(orderId)).Select(x => new OrderViewModel
                {
                    IBSOrderId = x.IBSOrderId,
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    PickupAddress = x.PickupAddress,
                    PickupDate = x.PickupDate,
                    Status = x.Status,
                    Title = FormatDateTime( x.PickupDate ), // piString.Format(titleFormat, x.IBSOrderId.ToString(), x.PickupDate),
                    IsFirst = x.Equals(Orders.First()),
                    IsLast = x.Equals(Orders.Last()),
                    ShowRightArrow = true
                }));
                HasOrders = Orders.Any();
            });
        }

		public async override void OnViewLoaded()
		{
			base.OnViewLoaded();
			await LoadOrders ();
		}

		public async Task LoadOrders ()
		{
			using (this.Services().Message.ShowProgress())
			{
				var orders = await _accountService.GetHistoryOrders();
				if (orders.Any())
				{
					var firstId = orders.First().Id;
					var lastId = orders.Last().Id;
					var ordersViewModels = new List<OrderViewModel>();
					foreach (var item in orders)
					{
						var viewModel = new OrderViewModel
						{
							IBSOrderId = item.IBSOrderId,
							Id = item.Id,
							CreatedDate = item.CreatedDate,
							PickupAddress = item.PickupAddress,
							PickupDate = item.PickupDate,
							Status = item.Status,
							Title = FormatDateTime(item.PickupDate),
							IsFirst = item.Id == firstId,
							IsLast = item.Id.Equals(lastId),
							ShowRightArrow = true
						};
						ordersViewModels.Add(viewModel);
					}
					Orders = new ObservableCollection<OrderViewModel>(ordersViewModels);
				}
				HasOrders = orders.Any();
			}
		}


		public ICommand NavigateToHistoryDetailPage
        {
            get
            {
                return this.GetCommand<OrderViewModel>(vm => ShowViewModel<HistoryDetailViewModel>(
                    new {orderId = vm.Id}));
            }
        }

        public override void OnViewUnloaded ()
        {
            base.OnViewUnloaded ();
            this.Services().MessengerHub.Unsubscribe<OrderDeleted>(_orderDeletedToken);
        }

    }
}