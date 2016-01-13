using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Braintree;
using apcurium.MK.Booking.Mobile.Extensions;
using UIKit;
using Cirrious.CrossCore;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class BraintreeVZeroService : IPaymentProviderClientService
    {
        


        public async Task<string> ShowDropInView(string clientToken)
        {
			var client = new BTAPIClient(clientToken);

			var viewControllerDelegate = new BraintreeDelegate();

            var paymentRequest = new BTPaymentRequest()
            {
                CallToActionText = "Save",
                AdditionalPayPalScopes = new NSSet<NSString>(new NSString("profile")),
                SummaryDescription = "Enter a new payment method for your account."
            };

            var dropInViewController = new BTDropInViewController(client)
			{
				Delegate = viewControllerDelegate,
                PaymentRequest = paymentRequest,
            };

			var cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel,(s,e) => 
			{
				viewControllerDelegate.DropInViewControllerDidCancel(dropInViewController);
			});

			dropInViewController.NavigationItem.LeftBarButtonItem = cancelButton;
            dropInViewController.NavigationItem.Title = "Add a payment method";
            dropInViewController.NavigationController.NavigationBarHidden = false;
            dropInViewController.NavigationItem.SetHidesBackButton(true, false);

            var navController = Mvx.Resolve<UINavigationController>();

            await navController.PresentViewControllerAsync(dropInViewController, true);

			return await viewControllerDelegate.GetTask();
        }

        public Task<string> GetPayPalNonce(string clientToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCreditCardNonce(string clientToken, string creditCardNumber, string ccv, string expirationMonth,
            string expirationYear, string firstName, string lastName, string zipCode)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPlatformPayNone(string clientToken)
        {
            throw new NotImplementedException();
        }


        private class BraintreeDelegate : BTDropInViewControllerDelegate
		{
			private readonly TaskCompletionSource<string> _taskCompletionSource;

			public BraintreeDelegate ()
			{
				_taskCompletionSource = new TaskCompletionSource<string>();
			}

			#region implemented abstract members of BTDropInViewControllerDelegate
			public override void DropInViewController(BTDropInViewController viewController, BTPaymentMethodNonce paymentMethodNonce)
			{
				_taskCompletionSource.TrySetResult(paymentMethodNonce.Nonce);
				viewController.DismissViewControllerAsync(true).FireAndForget();
			}
			public override void DropInViewControllerDidCancel(BTDropInViewController viewController)
			{
				_taskCompletionSource.TrySetCanceled();
				viewController.DismissViewControllerAsync(true).FireAndForget();
			}
			#endregion


			public Task<string> GetTask()
			{
				return _taskCompletionSource.Task;
			}
		}
    }
}
