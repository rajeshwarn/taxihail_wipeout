using System;
using System.Collections.ObjectModel;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using System.Threading.Tasks;
using System.Globalization;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {

        private ObservableCollection<OrderViewModel> _orders;
        private readonly TinyMessageSubscriptionToken _orderDeletedToken;

        public HistoryViewModel()
        {
            HasOrders = true; //Needs to be true otherwise we see the no order for a few seconds
            _orderDeletedToken = this.Services().MessengerHub.Subscribe<OrderDeleted>(c => OnOrderDeleted(c.Content));
            LoadOrders ();
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

        public void LoadOrders ()
		{
            this.Services().Account.GetHistoryOrders().Subscribe(orders =>
			{

                Orders = new ObservableCollection<OrderViewModel>(orders.Select(x => new OrderViewModel
                {
					IBSOrderId = x.IbsOrderId,
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    PickupAddress = x.PickupAddress,
                    PickupDate = x.PickupDate,
                    Status = x.Status,
                    Title = FormatDateTime(x.PickupDate),
                    IsFirst = x.Equals(orders.First()),
                    IsLast = x.Equals(orders.Last()),
                    ShowRightArrow = true
                }));
                HasOrders = orders.Any();

			});
		}


        public AsyncCommand<OrderViewModel> NavigateToHistoryDetailPage
        {
            get
            {
                return GetCommand<OrderViewModel>(vm => ShowViewModel<HistoryDetailViewModel>(
                    new {orderId = vm.Id}));
            }
        }

        public override void Unload ()
        {
            base.Unload ();
            this.Services().MessengerHub.Unsubscribe<OrderDeleted>(_orderDeletedToken);
        }

    }
}