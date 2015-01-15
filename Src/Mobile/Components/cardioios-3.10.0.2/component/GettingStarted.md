## Adding card.io to your iOS app

The most simple way to integrate card.io into your iOS app is to use the `PaymentViewController` just as you would any other UIViewController.  You will need to pass in an instance of `PaymentViewControllerDelegate` to the constructor so you can handle when a card is scanned, or the user has cancelled scanning.

```
// Create the delegate to handle events
var paymentDelegate = PaymentViewControllerDelegate();
paymentDelegate.OnScanCompleted += (viewController, cardInfo) => {

	if (cardInfo == null) {
		Console.WriteLine("Scanning Canceled!");
	} else {
		Console.WriteLine("Card Scanned: " + cardInfo.CardNumber);
	}	
	
	viewController.DismissViewController(true, null);
};

// Create and Show the View Controller
var paymentViewController = new PaymentViewController(paymentDelegate);

// Display the card.io interface
PresentViewController(paymentViewController, true);
```
