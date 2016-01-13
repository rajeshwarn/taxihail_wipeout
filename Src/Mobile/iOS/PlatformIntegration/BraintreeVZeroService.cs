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
        public Task<string> GetPayPalNonce(string clientToken)
        {
			var client = new BTAPIClient(clientToken);

			var navController = Mvx.Resolve<UINavigationController>();

			var paypalDriver = new BTPayPalDriver(client)
			{
					ViewControllerPresentingDelegate = navController
			};

			var tcs = new TaskCompletionSource<string>();

			paypalDriver.AuthorizeAccountWithAdditionalScopes(new NSSet("profile"), (paypalNonce, error) =>
				{
					if(error != null)
					{
						var exception = new NSErrorException(error);

						tcs.TrySetException(exception);
					}

					tcs.TrySetResult(paypalNonce.Nonce);
				});

			return tcs.Task;
        }

        public Task<string> GetCreditCardNonce(string clientToken, string creditCardNumber, string ccv, string expirationMonth,
            string expirationYear, string firstName, string lastName, string zipCode)
        {
			var braintreeClient = new BTAPIClient(clientToken);

			var cardClient = new BTCardClient(braintreeClient);

			var card = new BTCard(creditCardNumber, expirationMonth, expirationYear, ccv);
			card.PostalCode = zipCode;

			var tcs = new TaskCompletionSource<string>();

			cardClient.TokenizeCard(card, (nonce, error) =>
				{
					if(error != null)
					{
						var exception = new NSErrorException(error);

						tcs.TrySetException(exception);
					}

					tcs.TrySetResult(nonce.Nonce);
				});

			return tcs.Task;
        }

        public Task<string> GetPlatformPayNone(string clientToken)
        {
            throw new NotImplementedException();
        }
    }
}
