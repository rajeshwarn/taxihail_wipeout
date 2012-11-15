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

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class OrderCanceled: GenericTinyMessage<Order>
    {

        public OrderCanceled(object sender, Order order, string ownerId)
            : base(sender, order)
        {
            OwnerId = ownerId;
        }

        public string OwnerId { get; private set; }
    }
}