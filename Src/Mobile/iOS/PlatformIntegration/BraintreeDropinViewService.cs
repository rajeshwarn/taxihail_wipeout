using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Braintree;
using apcurium.MK.Booking.Mobile.Extensions;
using UIKit;
using Cirrious.CrossCore;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class BraintreeDropInViewService : IDropInViewService
    {
        public async Task<string> ShowDropInView(string clientToken)
        {
			var client = new BTAPIClient(clientToken);

			var viewControllerDelegate = new BraintreeDelegate();

            var dropInViewController = new BTDropInViewController(client)
			{
				Delegate = viewControllerDelegate
			};

			var cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel,(s,e) => 
				{
					viewControllerDelegate.DropInViewControllerDidCancel(dropInViewController);
				});

			dropInViewController.NavigationItem.LeftBarButtonItem = cancelButton;

            var navController = Mvx.Resolve<UINavigationController>();

            await navController.PresentViewControllerAsync(dropInViewController, true);

			return await viewControllerDelegate.GetTask();
        }




		private class BraintreeDelegate : BTDropInViewControllerDelegate
		{
			private TaskCompletionSource<string> _taskCompletionSource;

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
