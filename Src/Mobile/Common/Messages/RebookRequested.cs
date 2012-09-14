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
    public class RebookRequested : GenericTinyMessage<Order>
    {
        public RebookRequested(object sender, Order order)
            : base(sender, order)
        {            
        }
    }
}