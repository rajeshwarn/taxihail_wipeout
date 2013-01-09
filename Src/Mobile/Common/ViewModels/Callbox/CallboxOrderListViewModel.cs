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
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderListViewModel : BaseCallboxViewModel
    {
        private TinyMessageSubscriptionToken token;
        public CreateOrder Order { get; private set; }

        private ObservableCollection<CallboxOrderViewModel> _orders { get; set; }

        public ObservableCollection<CallboxOrderViewModel> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        } 

        public string ApplicationName
        {
            get { return AppSettings.ApplicationName; }
        }

        public CallboxOrderListViewModel() : base()
        {
            Order = new CreateOrder();
            Order.Settings = AccountService.CurrentAccount.Settings;
            Orders = CacheService.Get<ObservableCollection<CallboxOrderViewModel>>("callbox.orderList") ?? new ObservableCollection<CallboxOrderViewModel>();
            token = this.MessengerHub.Subscribe<OrderDeleted>(orderId =>
            {
                this.CancelOrder.Execute(orderId.Content);
                OrderCompleted(this, null);
            });
        }

        

        protected override void Close()
        {
            base.Close();
            token.Dispose();
        }
 
        public CallboxOrderListViewModel(string passengerName)
            : this()
        {
            CreateCommand.Execute(passengerName);
        }

        public IMvxCommand CallTaxi
        {
            get
            {
                return this.GetCommand(() => this.MessageService.ShowEditTextDialog(Resources.GetString("BookTaxiTitle"), Resources.GetString("BookTaxiPassengerName"), Resources.GetString("Ok"), passengerName => CreateCommand.Execute(passengerName)));
            }
        }

        public IMvxCommand CancelOrder
        {
            get
            {
                return this.GetCommand<Guid>(orderId => this.MessageService.ShowMessage(this.Resources.GetString("CancelOrderTitle"), 
                    this.Resources.GetString("CancelOrderMessage"), 
                    this.Resources.GetString("Yes"), ()
                    =>
                        {
                            BookingService.CancelOrder(orderId);
                            var orderToRemove = Orders.FirstOrDefault(o => o.Id.Equals(orderId));
                            Orders.Remove(orderToRemove);
                            CacheService.Set("callbox.orderList", Orders);
                            if (Orders.Count == 0)
                            {
                                RequestNavigate<CallboxCallTaxiViewModel>();
                                this.Close();
                            }
                        }, this.Resources.GetString("No"), () => { }));
            }
        }

        public IMvxCommand CreateCommand
        {
            get
            {
                return GetCommand<string>(passengerName =>
                                              {
                                                  Order.Id = Guid.NewGuid();
                                                  var pickupAddress =
                                                      AccountService.GetFavoriteAddresses().FirstOrDefault();
                                                  if (pickupAddress != null)
                                                  {
                                                      Order.PickupAddress = pickupAddress;
                                                      Order.PickupDate = DateTime.Now;
                                                      Order.Note =
                                                          string.Format(Resources.GetString("Callbox.passengerName"),
                                                                        passengerName);
                                                      try
                                                      {
                                                          MessageService.ShowProgress(true);
                                                          var orderInfo = BookingService.CreateOrder(Order);

                                                          if (orderInfo.IBSOrderId.HasValue
                                                              && orderInfo.IBSOrderId > 0)
                                                          {
                                                              InvokeOnMainThread(() =>
                                                                                     {
                                                                                         Orders.Add(new CallboxOrderViewModel()
                                                                                         {
                                                                                             CreatedDate = DateTime.Now,
                                                                                             IbsOrderId = orderInfo.IBSOrderId,
                                                                                             Id = Order.Id,
                                                                                             OrderStatus = orderInfo
                                                                                         });
                                                                                         CacheService.Set("callbox.orderList", Orders);
                                                                                     });
                                                          }
                                                      }
                                                      catch (Exception ex)
                                                      {
                                                          InvokeOnMainThread(() =>
                                                                                 {
                                                                                     var settings =
                                                                                         TinyIoCContainer.Current
                                                                                                         .Resolve
                                                                                             <IAppSettings>();
                                                                                     string err =
                                                                                         string.Format(
                                                                                             Resources.GetString(
                                                                                                 "ServiceError_ErrorCreatingOrderMessage"),
                                                                                             settings.ApplicationName,
                                                                                             settings.PhoneNumberDisplay
                                                                                                 (
                                                                                                     Order.Settings
                                                                                                          .ProviderId
                                                                                                          .HasValue
                                                                                                         ? Order
                                                                                                               .Settings
                                                                                                               .ProviderId
                                                                                                               .Value
                                                                                                         : 1));
                                                                                     MessageService.ShowMessage(
                                                                                         Resources.GetString(
                                                                                             "ErrorCreatingOrderTitle"),
                                                                                         err);
                                                                                 });
                                                      }
                                                      finally
                                                      {
                                                          MessageService.ShowProgress(false);
                                                      }
                                                  }
                                                  else
                                                  {
                                                      MessageService.ShowMessage(
                                                          Resources.GetString("ErrorCreatingOrderTitle"),
                                                          Resources.GetString("NoPickupAddress"));
                                                  }
                                              });
            }
        }

        public delegate void OrderHandler(object sender, EventArgs args);
        public event OrderHandler OrderCompleted ;
        
    }

}