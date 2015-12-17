using System;
using Cirrious.CrossCore.Droid.Platform;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using Braintree.Api;
using Cirrious.MvvmCross.Droid.Fragging;
using Braintree.Api.Models;

namespace TaxiHail.Shared.PlatformIntegration
{
	public class BraintreeDropinViewService : IBraintreeDropinViewService
	{
		private readonly IMvxAndroidCurrentTopActivity _activity;
		private readonly IPaymentServiceClient _paymentService;
		private const int _requestCode = 52120;

		public BraintreeDropinViewService(IMvxAndroidCurrentTopActivity activity, IPaymentServiceClient paymentService)
		{
			_paymentService = paymentService;
			_activity = activity;
		}

		public Task<string> ShowDropinView(string clientToken)
		{
			var paymentRequest = new PaymentRequest()
				.ActionBarTitle("Add a payment method")
				.PrimaryDescription("Enter a new payment method for your account.")
				.SubmitButtonText("Save")
				.ClientToken(clientToken);

			var tcs = new TaskCompletionSource<string>();

			var activity = (MvxFragmentActivity)_activity.Activity;

			activity.StartActivityForResultCalled += (sender, e) => 
			{
				if(e.Value.RequestCode == _requestCode)
				{
					var paymentNonce = (PaymentMethodNonce)e.Value.Intent.GetParcelableExtra(BraintreePaymentActivity.ExtraPaymentMethodNonce);
					
					tcs.SetResult(paymentNonce.Nonce);
				}
			};
					
			activity.StartActivityForResult(paymentRequest.GetIntent(activity), _requestCode);

			return tcs.Task;
		}
	}
}

