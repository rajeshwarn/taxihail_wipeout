using System;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PayPalViewModel : BaseViewModel
    {
        public PayPalViewModel (string url)
        {
            Url = url;
        }

        public string Url { get; set; }
    }
}

