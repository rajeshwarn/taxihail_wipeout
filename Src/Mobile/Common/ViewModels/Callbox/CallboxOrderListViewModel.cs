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
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderListViewModel : BaseCallboxViewModel
    {
        private TinyMessageSubscriptionToken token;
        public CreateOrder Order { get; private set; }
        private IDisposable refreshStatusToken;
        private ObservableCollection<CallboxOrderViewModel> _orders { get; set; }
        private List<int?> OrderNotified { get; set; } 
		private bool _isOnErrorState{get;set;}
		private Guid _orderIdOnError{get;set;}
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
			_orderIdOnError = Guid.Empty;
             refreshStatusToken = Observable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(20))
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
                    if (!Orders.Any() && !_isOnErrorState)
                    {
                        RequestNavigate<CallboxCallTaxiViewModel>();
                        this.Close();
                    }
					if(Orders.Any(o=>o.Id == _orderIdOnError))
					{
						_isOnErrorState= false;
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
                Logger.LogError(e);
            }
             
           
        }

        protected override void Close()
        {
            base.Close();
			UnsubscribeToken();
        }

		public void UnsubscribeToken()
		{
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
                            var orderToRemove = Orders.FirstOrDefault(o => o.Id.Equals(orderId));
					
					Task.Factory.StartNew(()=>{

                            BookingService.CancelOrder(orderId);
					}).ContinueWith(t=>{
						MessageService.ShowMessage(this.Resources.GetString("ServiceError_ErrorCreatingOrderMessage"),this.Resources.GetString("ErrorCancellingOrderTitle"));
						Orders.Add(orderToRemove);
						
					},TaskContinuationOptions.OnlyOnFaulted);

						MessageService.ShowProgress(true);
                            Orders.Remove(orderToRemove);
                            CacheService.Set("callbox.orderList", Orders);
                            if (Orders.Count == 0)
                            {
                                RequestNavigate<CallboxCallTaxiViewModel>();
                                this.Close();
                            }
					MessageService.ShowProgress(false);
			
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
                                                          Order.Settings.Name = passengerName;
                                                      }
                                                      else
                                                      {
                                                          Order.Note = Resources.GetString("Callbox.noPassengerName");
                                                          Order.Settings.Name = Resources.GetString("NotSpecified");
                                                      }
                                                      
                                                      try
                                                      {
                                                          MessageService.ShowProgress(true);
                                                          var orderInfo = BookingService.CreateOrder(Order);

                                                          if (orderInfo.IBSOrderId.HasValue
                                                              && orderInfo.IBSOrderId > 0)
                                                          {
                                                              orderInfo.Name = Order.Settings.Name;
                                                              InvokeOnMainThread(() =>
                                                                                     {
                                                                                         Orders.Insert(0,new CallboxOrderViewModel()
                                                                                         {
                                                                                             CreatedDate = DateTime.Now,
                                                                                             IbsOrderId = orderInfo.IBSOrderId,
                                                                                             Id = Order.Id,
                                                                                             OrderStatus = orderInfo
                                                                                         });
                                                                                         CacheService.Set("callbox.orderList", Orders);
									_isOnErrorState = false;
                                                                                     });
                                                          }
                                                      }
                                                      catch (Exception ex)
                                                      {
                                                          InvokeOnMainThread(() =>
                                                                                 {
								_isOnErrorState = true;
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