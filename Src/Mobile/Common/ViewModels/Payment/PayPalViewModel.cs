using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class PayPalViewModel : PageViewModel, ISubViewModel<bool>
    {
		public new void Init(string url)
        {
            Url = url;
        }

        public string Url { get; private set; }

		private bool _webViewLoadFinishedOnce;
		public void WebViewLoadFinished ()
		{
			if (!_webViewLoadFinishedOnce) 
			{
                this.Services().Message.ShowProgress(false);
				_webViewLoadFinishedOnce = true;
			}
		}

		public ICommand Finish
		{
			get
			{
				return this.GetCommand<bool>(b => this.ReturnResult(b));
			}
		}
    }
}

