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
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Models
{
    public class OrderWithStatusModel
    {
        public Order Order { get; set; }
        public OrderStatusDetail OrderStatusDetail { get; set; }
    }
}