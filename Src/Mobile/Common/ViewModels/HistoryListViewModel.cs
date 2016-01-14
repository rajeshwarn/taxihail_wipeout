using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using MK.Common.Exceptions;
using System.Net;
using apcurium.MK.Common;
using Cirrious.CrossCore;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HistoryListViewModel : PageViewModel
    {
		private readonly IAccountService _accountService;

		public HistoryListViewModel(IAccountService accountService)
        {
			_accountService = accountService;
        }

		private ObservableCollection<OrderViewModel> _orders;

		private TinyMessageSubscriptionToken _orderDeletedToken;
        private TinyMessageSubscriptionToken _orderStatusChangedToken;

		public void Init()
		{
			HasOrders = true; //Needs to be true otherwise we see the no order for a few seconds 

            var services = this.Services();
            var msgHub = services.MessengerHub;

            _orderDeletedToken = msgHub.Subscribe<OrderDeleted>(c => OnOrderDeleted(c.Content));
            _orderStatusChangedToken = msgHub.Subscribe<OrderStatusChanged>(c => OnOrderStatusChanged(c.Content, c.Status));
		}

        public ObservableCollection<OrderViewModel> Orders
        {
            get { return _orders; }
			set { _orders = value; RaisePropertyChanged(); }
        }

        private string FormatDateTime(DateTime date )
        {
			var formatTime = CultureProvider.CultureInfo.DateTimeFormat.ShortTimePattern;
            var format = "{0:dddd, MMMM d}, {0:"+formatTime+"}";
            var result = string.Format(format, date) ;
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

	    private void OnOrderStatusChanged(Guid orderId, OrderStatus status)
	    {
            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
            }
	    }

        private void OnOrderDeleted(Guid orderId)
        {
            Orders.Remove(x => x.Id == orderId);
            HasOrders = Orders.Any();
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
			    IList<Order> orders = new Order[0];

			    try
			    {
                    var allOrders = await _accountService.GetHistoryOrders();

                    // MKTAXI - 2589
                    orders = allOrders.ToArray();
                }
			    catch (Exception ex)
				{
					Logger.LogMessage(ex.Message, ex.ToString());

					if(!Mvx.Resolve<IErrorHandler>().HandleError(ex))
					{
						this.Services().Message.ShowMessage(this.Services().Localize["Error"], this.Services().Localize["HistoryLoadError"]);
					}
			    }

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
            this.Services().MessengerHub.Unsubscribe<OrderStatusChanged>(_orderStatusChangedToken);
        }
    }
}