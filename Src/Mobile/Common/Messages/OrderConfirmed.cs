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
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Mobile.Messages
{
    public class OrderConfirmed : GenericTinyMessage<CreateOrder>
    {
        public OrderConfirmed(object sender, CreateOrder address)
            : base(sender, address)
        {            
        }
    }
}