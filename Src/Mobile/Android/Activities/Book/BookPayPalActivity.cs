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
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Mobile.Client.Activities;
using Android.Webkit;

namespace apcurium.MK.Booking.Mobile.Client
{
	 [Activity(Label = "BookPayPalActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	 public class BookPayPalActivity : BaseBindingActivity<PayPalViewModel>
	 {
		protected override int ViewTitleResourceId
		{
			get { return Resource.String.View_PaymentCreditCardsOnFile; }
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
		}

		protected override void OnViewModelSet()
		{            
			ViewModel.Load();		

			var webView = new WebView(this);
			webView.SetWebViewClient(new PayPalWebViewClient(ViewModel));
			SetContentView(webView);
			webView.LoadUrl(ViewModel.Url);
		}
	}

	public class PayPalWebViewClient: WebViewClient
	{
		readonly PayPalViewModel _viewModel;
		public PayPalWebViewClient (PayPalViewModel viewModel)
		{
			_viewModel = viewModel;
			
		}
		public override void OnPageFinished (WebView view, string url)
		{
			base.OnPageFinished (view, url);
			_viewModel.WebViewLoadFinished ();
		}

		public override bool ShouldOverrideUrlLoading (WebView view, string url)
		{
			if(url.StartsWith("taxihail"))
			{
				if (url.EndsWith("success")) {
					_viewModel.Finish.Execute (true);
				} else {
					_viewModel.Finish.Execute (false);
				}
				return true;
			}
			return base.ShouldOverrideUrlLoading (view, url);
		}
	}
}

