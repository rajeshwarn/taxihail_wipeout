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
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Extensions;
using System.Threading.Tasks;
using System.Globalization;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {

        private ObservableCollection<OrderViewModel> _orders;
        private readonly TinyMessageSubscriptionToken _orderDeletedToken;

        public HistoryViewModel()
        {
            HasOrders = true; //Needs to be true otherwise we see the no order for a few seconds
            _orderDeletedToken = MessengerHub.Subscribe<OrderDeleted>(c => OnOrderDeleted(c.Content));
            LoadOrders ();
        }
        public ObservableCollection<OrderViewModel> Orders
        {
            get { return _orders; }
            set { _orders = value; FirePropertyChanged("Orders"); }
        }

        private string FormatDateTime(DateTime date )
        {
            var formatTime = new CultureInfo( CultureInfoString ).DateTimeFormat.ShortTimePattern;
            string format = "{0:dddd, MMMM d}, {0:"+formatTime+"}";
            string result = string.Format(format, date) ;
            return result;
        }
        public string CultureInfoString
        {
            get{
                var culture = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting ( "PriceFormat" );
                if ( culture.IsNullOrEmpty() )
                {
                    return "en-US";
                }
                return culture;
            }
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



        private void OnOrderDeleted(Guid orderId)
        {
            Task.Factory.StartNew(() =>
            {
                Orders = new ObservableCollection<OrderViewModel>(Orders.Where(order=>!order.Id.Equals(orderId)).Select(x => new OrderViewModel()
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
			AccountService.GetHistoryOrders ().Subscribe(orders =>
			{

                Orders = new ObservableCollection<OrderViewModel>(orders.Select(x => new OrderViewModel()
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


        public IMvxCommand NavigateToHistoryDetailPage
        {
            get
            {
                return GetCommand<OrderViewModel>(vm => RequestNavigate<HistoryDetailViewModel>(
                    new {orderId = vm.Id}));
            }
        }

        public override void Unload ()
        {
            base.Unload ();
            MessengerHub.Unsubscribe<OrderDeleted>(_orderDeletedToken);
        }

    }
}