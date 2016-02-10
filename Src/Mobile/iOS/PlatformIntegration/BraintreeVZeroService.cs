using System;
using System.Threading.Tasks;
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
        public async Task<string> GetPayPalNonce(string clientToken)
        {
			var client = new BTAPIClient(clientToken);

			var navController = Mvx.Resolve<UINavigationController>();

			var paypalDriver = new BTPayPalDriver(client)
			{
				ViewControllerPresentingDelegate = new PresentingDelegate(navController),
				ReturnURLScheme = NSBundle.MainBundle.BundleIdentifier + ".paypal"
			};

			var nonce = await paypalDriver.AuthorizeAccountWithAdditionalScopesAsync(new string[]{ "profile" });

			return nonce.Nonce;
        }

        public async Task<string> GetCreditCardNonce(string clientToken, string creditCardNumber, string ccv, string expirationMonth,
            string expirationYear, string firstName, string lastName, string zipCode)
        {
			var braintreeClient = new BTAPIClient(clientToken);

			var cardClient = new BTCardClient(braintreeClient);

			var card = new BTCard(creditCardNumber, expirationMonth, expirationYear, ccv);
			card.PostalCode = zipCode;

			var tcs = new TaskCompletionSource<string>();

			var nonce = await cardClient.TokenizeCardAsync(card);

			return nonce.Nonce;
        }

        public Task<string> GetPlatformPayNonce(string clientToken)
        {
            throw new NotImplementedException("Apple pay is not currently implemented. This will be implemented in a future release.");
        }
        
		private class PresentingDelegate: BTViewControllerPresentingDelegate
		{
			private UINavigationController _navController;

			public PresentingDelegate(UINavigationController navController)
			{
				_navController = navController;
			}

			#region implemented abstract members of BTViewControllerPresentingDelegate
			public override void RequestsDismissalOfViewController(NSObject driver, UIViewController viewController)
			{
				_navController.DismissViewControllerAsync(true).FireAndForget();
			}
			public override void RequestsPresentationOfViewController(NSObject driver, UIViewController viewController)
			{
				_navController.PresentViewControllerAsync(viewController, true).FireAndForget();
			}
			#endregion
		}
    }
}
