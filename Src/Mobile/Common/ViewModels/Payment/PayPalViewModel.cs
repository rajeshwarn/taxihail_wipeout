using apcurium.MK.Booking.Mobile.Extensions;

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

		public override void Load ()
		{
			base.Load ();
			// Show progress indicator while loading first request
            this.Services().Message.ShowProgress(true);
		}

		private bool _webViewLoadFinishedOnce;
		public void WebViewLoadFinished ()
		{
			if (!_webViewLoadFinishedOnce) {
                this.Services().Message.ShowProgress(false);
				_webViewLoadFinishedOnce = true;
			}
		}

        //todo : declare command in basesubviewmodel and refactor the Finish pattern in other viewmodels
        public AsyncCommand<bool> Finish
		{
			get{
				return GetCommand<bool>(ReturnResult);
			}
		}
    }
}

