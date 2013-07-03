using System;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Reactive.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PayPalViewModel : BaseViewModel
    {
        public PayPalViewModel (string url)
        {
            Url = url;
        }

        public string Url { get; private set; }
    }
}

