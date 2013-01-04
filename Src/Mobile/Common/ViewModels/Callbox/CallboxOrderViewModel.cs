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

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderViewModel
    {
        public Guid Id { get; set; }

        public int? IbsOrderId { get; set; }

        public DateTime CreatedDate { get; set; }

        public OrderStatusDetail OrderStatus { get; set; }

    }
}