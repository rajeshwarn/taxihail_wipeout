using System;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PayPalViewModel : BaseSubViewModel<bool>
    {
        public PayPalViewModel (string url, string messageId)
			:base(messageId)
        {
            Url = url;
        }

        public string Url { get; private set; }
		public IMvxCommand Finish
		{
			get{
				return GetCommand (() => {
					ReturnResult(true);
				});
			}
		}
    }
}

