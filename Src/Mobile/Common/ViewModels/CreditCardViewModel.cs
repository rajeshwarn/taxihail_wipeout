using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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