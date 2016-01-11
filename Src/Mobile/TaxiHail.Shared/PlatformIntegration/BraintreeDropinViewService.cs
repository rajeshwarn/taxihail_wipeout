using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Cirrious.CrossCore.Droid.Platform;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.App;
using Braintree.Api;
using Braintree.Api.Models;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Droid.Views;
using Cirrious.MvvmCross.Droid.Views;
using Observable = System.Reactive.Linq.Observable;

namespace TaxiHail.Shared.PlatformIntegration
{
	public class BraintreeDropInViewService : IDropInViewService
	{
		private readonly IMvxAndroidCurrentTopActivity _activity;
	    private const int _requestCode = 52120;

		public BraintreeDropInViewService(IMvxAndroidCurrentTopActivity activity)
		{
		    _activity = activity;
		}

		public Task<string> ShowDropInView(string clientToken)
		{
			var paymentRequest = new PaymentRequest()
				.ActionBarTitle("Add a payment method")
                .PaypalAdditionalScopes(new[]
                {
                    "profile"
                })
                .PrimaryDescription("Enter a new payment method for your account.")
				.SubmitButtonText("Save")
				.ClientToken(clientToken);


		    var eventSourceActivity = (IMvxEventSourceActivity) _activity.Activity;
                

            var activtyResultObservable = Observable.FromEventPattern<EventHandler<MvxValueEventArgs<MvxActivityResultParameters>>, MvxValueEventArgs<MvxActivityResultParameters>>
                (
                    h => eventSourceActivity.ActivityResultCalled += h,
                    h => eventSourceActivity.ActivityResultCalled -= h
                )
                .Select(result => result.EventArgs.Value)
                .Where(args => args.RequestCode == _requestCode);

            var activity = _activity.Activity;
            activity.StartActivityForResult(paymentRequest.GetIntent(activity), _requestCode);

		    return activtyResultObservable
                .Take(1) // This forces to complete the observable flow.
                .Do(args =>
                {
                    if (args.ResultCode == Result.Canceled || args.Data == null)
                    {
                        throw new TaskCanceledException("Braintree flow was cancelled by user");
                    }
                })
                .Select(args => (PaymentMethodNonce)args.Data.GetParcelableExtra(BraintreePaymentActivity.ExtraPaymentMethodNonce))
                .Select(paymentMethodNonce => paymentMethodNonce.Nonce)
                .ToTask();
		}
	}
}

