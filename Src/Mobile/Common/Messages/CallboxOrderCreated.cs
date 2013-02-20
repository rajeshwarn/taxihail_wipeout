using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class CallboxOrderCreated : TinyMessageBase
    {
        public Order Order;
        public OrderStatusDetail OrderStatus;
        public CallboxOrderCreated(object sender, Order order, OrderStatusDetail orderStatus) : base(sender)
        {
            Order = order;
            OrderStatus = orderStatus;
        }
    }
}