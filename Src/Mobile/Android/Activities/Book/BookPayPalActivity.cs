using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Webkit;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "BookPayPalActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookPayPalActivity : BaseBindingActivity<PayPalViewModel>
    {
        private WebView _webView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
        }

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            _webView = new WebView(this);
            _webView.SetWebViewClient(new PayPalWebViewClient(ViewModel));
            SetContentView(_webView);
            _webView.Settings.JavaScriptEnabled = true;

            _webView.LoadUrl(ViewModel.Url);
        }
    }

    public class PayPalWebViewClient : WebViewClient
    {
        private readonly PayPalViewModel _viewModel;

        public PayPalWebViewClient(PayPalViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            _viewModel.WebViewLoadFinished();
        }

        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            if (url.StartsWith("taxihail"))
            {
                _viewModel.Finish.Execute(url.EndsWith("success"));
                return true;
            }
            return base.ShouldOverrideUrlLoading(view, url);
        }
    }
}