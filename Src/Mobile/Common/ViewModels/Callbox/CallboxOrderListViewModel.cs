using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Cirrious.MvvmCross.Interfaces.Commands;
using ServiceStack.ServiceClient.Web;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderListViewModel : BaseCallboxViewModel
    {
        private TinyMessageSubscriptionToken token;
        public CreateOrder Order { get; private set; }
        private IDisposable refreshStatusToken;
        private ObservableCollection<CallboxOrderViewModel> _orders { get; set; }
        private List<int?> OrderNotified { get; set; } 

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
            OrderNotified = new List<int?>();
            Order.Settings = AccountService.CurrentAccount.Settings;
           // Orders = CacheService.Get<ObservableCollection<CallboxOrderViewModel>>("callbox.orderList") ?? new ObservableCollection<CallboxOrderViewModel>();
            //var orderStatus = AccountService.GetActiveOrdersStatus().ToList();
            Orders = new ObservableCollection<CallboxOrderViewModel>();
             refreshStatusToken = Observable.Timer(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20))
                      .Subscribe(a =>
                                     {
                                         RefreshOrderStatus();
                                     });
            token = this.MessengerHub.Subscribe<OrderDeleted>(orderId =>
            {
                this.CancelOrder.Execute(orderId.Content);
            });
        }

        private void RefreshOrderStatus()
        {
            try
            {
                var orderStatus = AccountService.GetActiveOrdersStatus().ToList().OrderByDescending(o => o.IBSOrderId);
                InvokeOnMainThread(() =>
                {
                    Orders.Clear();
                    Orders.AddRange(orderStatus.Where(status => BookingService.IsCallboxStatusActive(status.IBSStatusId)).Select(status => new CallboxOrderViewModel()
                    {
                        OrderStatus = status,
                        CreatedDate = status.PickupDate,
                        IbsOrderId = status.IBSOrderId,
                        Id = status.OrderId
                    }));
                    if (!Orders.Any())
                    {
                        RequestNavigate<CallboxCallTaxiViewModel>();
                        this.Close();
                    }
                    foreach (var order in Orders)
                    {
                        if (BookingService.IsCallboxStatusCompleted(order.OrderStatus.IBSStatusId) && !OrderNotified.Any(c => c.Value.Equals(order.IbsOrderId)))
                        {
                            OrderNotified.Add(order.IbsOrderId);
                            OrderCompleted(this, null);
                        }
                    }
                });
            }
            catch (WebServiceException e)
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
                                                       "ServiceError_ErrorRefreshingOrderMessage"),
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
                                                   "ErrorRefreshingOrderTitle"),
                                               err);
                                       });
            }
             
           
        }

        protected override void Close()
        {
            base.Close();
            token.Dispose();
            refreshStatusToken.Dispose();
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
                                                      if (!string.IsNullOrEmpty(passengerName))
                                                      {
                                                          Order.Note =
                                                          string.Format(Resources.GetString("Callbox.passengerName"),
                                                                        passengerName);
                                                      }
                                                      else
                                                      {
                                                          Order.Note = Resources.GetString("Callbox.noPassengerName");
                                                      }
                                                      
                                                      try
                                                      {
                                                          MessageService.ShowProgress(true);
                                                          var orderInfo = BookingService.CreateOrder(Order);

                                                          if (orderInfo.IBSOrderId.HasValue
                                                              && orderInfo.IBSOrderId > 0)
                                                          {
                                                              InvokeOnMainThread(() =>
                                                                                     {
                                                                                         Orders.Insert(0,new CallboxOrderViewModel()
                                                                                         {
                                                                                             CreatedDate = orderInfo.PickupDate,
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