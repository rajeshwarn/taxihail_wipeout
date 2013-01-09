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
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderListViewModel : BaseCallboxViewModel
    {
        
        private ObservableCollection<CallboxOrderViewModel> _orders { get; set; }

        public ObservableCollection<CallboxOrderViewModel> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        } 

        public CallboxOrderListViewModel() : base()
        {
            Orders = new ObservableCollection<CallboxOrderViewModel>();
        }

        public CallboxOrderListViewModel(string order, string orderStatus) : base()
        {
            Orders = new ObservableCollection<CallboxOrderViewModel>();
           var orderDeserialize= JsonSerializer.DeserializeFromString<Order> (order);
           var orderStatusDeserialize = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);      
            Orders.Add(new CallboxOrderViewModel(){
                CreatedDate = orderDeserialize.CreatedDate,
                IbsOrderId  = orderDeserialize.IBSOrderId,
                Id = orderDeserialize.Id,
                OrderStatus = orderStatusDeserialize});
            this.MessengerHub.Subscribe<OrderDeleted>(orderId =>
                                                          {
                                                              this.CancelOrder.Execute(orderId.Content);
                                                              OrderCompleted(this, null);
                                                          });
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
                            _bookingService.CancelOrder(orderId);
                            var orderToRemove = Orders.FirstOrDefault(o => o.Id.Equals(orderId));
                            Orders.Remove(orderToRemove);
                            if (Orders.Count == 0)
                            {
                                RequestNavigate<CallboxCallTaxiViewModel>();
                            }
                        }, this.Resources.GetString("No"), () => { }));
            }
        }
        public delegate void OrderHandler(object sender, EventArgs args);
        public event OrderHandler OrderCompleted ;
        
    }

}