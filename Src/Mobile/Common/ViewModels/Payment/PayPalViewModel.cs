using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PayPalViewModel : BaseSubViewModel<bool>
    {
		public void Init(string url, string messageId)
        {
			Init(messageId);

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

        public AsyncCommand<bool> Finish
		{
			get{
				return GetCommand<bool>(ReturnResult);
			}
		}
    }
}

