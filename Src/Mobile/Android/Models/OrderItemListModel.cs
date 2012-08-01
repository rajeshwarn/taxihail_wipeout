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

namespace apcurium.MK.Booking.Mobile.Client.Models
{
    public class OrderItemListModel
    {
        public Order Order { get; set; }
        public int BgResource { get; set; }
        public int ImageResource { get; set; }
    }
}