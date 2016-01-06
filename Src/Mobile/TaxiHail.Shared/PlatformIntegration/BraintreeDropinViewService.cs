using Cirrious.CrossCore.Droid.Platform;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.App;
using Braintree.Api;
using Braintree.Api.Models;
using Cirrious.MvvmCross.Droid.Views;

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

			var tcs = new TaskCompletionSource<string>();

			var activity = (MvxActivity)_activity.Activity;

			activity.ActivityResultCalled += (sender, e) => 
			{
			    if (e.Value.RequestCode != _requestCode)
			    {
			        return;
			    }
                
			    if (e.Value.ResultCode == Result.Canceled)
			    {
                    tcs.SetCanceled();
                    return;
                }

			    if (e.Value.Data == null)
			    {
                    tcs.SetCanceled();
                    return;
                }
				
			    var paymentNonce = (PaymentMethodNonce)e.Value.Data.GetParcelableExtra(BraintreePaymentActivity.ExtraPaymentMethodNonce);

			    tcs.SetResult(paymentNonce.Nonce);
			};

			activity.StartActivityForResult(paymentRequest.GetIntent(activity), _requestCode);

			return tcs.Task;
		}
	}
}

