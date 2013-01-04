using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderListViewModel : BaseViewModel, IMvxServiceConsumer<IBookingService>,
        IMvxServiceConsumer<IAccountService>
    {
        private IBookingService _bookingService;
        private IAccountService _accountService;
        public CreateOrder Order { get; private set; }
        private ObservableCollection<CallboxOrderViewModel> _orders { get; set; }

        public ObservableCollection<CallboxOrderViewModel> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        } 

        public CallboxOrderListViewModel()
        {
            Orders = new ObservableCollection<CallboxOrderViewModel>();
        }

        public CallboxOrderListViewModel(string order, string orderStatus)
        {
            Orders = new ObservableCollection<CallboxOrderViewModel>();
           var orderDeserialize= JsonSerializer.DeserializeFromString<Order> (order);
           var orderStatusDeserialize = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);      
            Orders.Add(new CallboxOrderViewModel(){
                CreatedDate = orderDeserialize.CreatedDate,
                IbsOrderId  = orderDeserialize.IBSOrderId,
                Id = orderDeserialize.Id,
                OrderStatus = orderStatusDeserialize});
        }


        public IMvxCommand CallTaxi
        {
            get
            {
                return this.GetCommand(() => this.MessageService.ShowEditTextDialog(Resources.GetString("BookTaxiTitle"), Resources.GetString("BookTaxiPassengerName"), Resources.GetString("Ok"), CreateCommand));
            }
        }

        private void CreateCommand(string passengerName)
        {
            Order.Id = Guid.NewGuid();
            var pickupAddress = _accountService.GetFavoriteAddresses().FirstOrDefault();
            if (pickupAddress != null)
            {
                Order.PickupAddress = pickupAddress;
                Order.PickupDate = DateTime.Now;
                Order.Note = string.Format(Resources.GetString("Callbox.passengerName"), passengerName);
                try
                {
                    MessageService.ShowProgress(true);
                    var orderInfo = _bookingService.CreateOrder(Order);

                    if (orderInfo.IBSOrderId.HasValue
                        && orderInfo.IBSOrderId > 0)
                    {
                        var orderCreated = new Order
                        {
                            CreatedDate = DateTime.Now,
                            DropOffAddress = Order.DropOffAddress,
                            IBSOrderId = orderInfo.IBSOrderId,
                            Id = Order.Id,
                            PickupAddress = Order.PickupAddress,
                            Note = Order.Note,
                            PickupDate =
                                Order.PickupDate.HasValue
                                    ? Order.PickupDate.Value
                                    : DateTime.Now,
                            Settings = Order.Settings
                        };
                        RequestNavigate<CallboxOrderListViewModel>(new
                        {
                            order = orderCreated.ToJson(),
                            orderStatus = orderInfo.ToJson()
                        });
                        Close();
                        MessengerHub.Publish(new OrderConfirmed(this, Order, false));
                    }

                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
                        string err =
                            string.Format(
                                Resources.GetString("ServiceError_ErrorCreatingOrderMessage"),
                                settings.ApplicationName,
                                settings.PhoneNumberDisplay(Order.Settings.ProviderId.HasValue
                                                                ? Order.Settings.ProviderId.Value
                                                                : 1));
                        MessageService.ShowMessage(
                            Resources.GetString("ErrorCreatingOrderTitle"), err);
                    });
                }
                finally
                {
                    MessageService.ShowProgress(false);
                }
            }
            else
            {
                MessageService.ShowMessage(Resources.GetString("ErrorCreatingOrderTitle"), "No favorite address");
            }
        }
    }
}