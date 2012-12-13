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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CreditCardViewModel : CreditCardDetails
    {
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public bool ShowPlusSign { get; set; }
        public bool IsAddNew { get; set; }
        public string Picture { get; set; }
    }
}