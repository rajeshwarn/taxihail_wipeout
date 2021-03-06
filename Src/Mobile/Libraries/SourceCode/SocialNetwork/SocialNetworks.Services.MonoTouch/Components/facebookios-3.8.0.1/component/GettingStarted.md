The Facebook SDK for iOS makes it easier and faster to develop Facebook integrated iOS apps.

## Sample

### AppDelegate.cs

```csharp
using MonoTouch.FacebookConnect;
//...

// Get your own App ID at developers.facebook.com/apps
const string FacebookAppId = "Your-Id-Here";
const string DisplayName = "Your_App_Display_Name";

public override bool FinishedLaunching (UIApplication app, NSDictionary options)
{
	FBSettings.DefaultAppID = FacebookAppId;
	FBSettings.DefaultDisplayName = DisplayName;
	//...
}

public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
{
	// We need to handle URLs by passing them to FBSession in order for SSO authentication
	// to work.
	return FBSession.ActiveSession.HandleOpenURL(url);
}

public override void OnActivated (UIApplication application)
{
	// We need to properly handle activation of the application with regards to SSO
	// (e.g., returning from iOS 6.0 authorization dialog or from fast app switching).
	FBSession.ActiveSession.HandleDidBecomeActive();
}

```

### YourViewController.cs

```csharp
using MonoTouch.FacebookConnect;
//...

// For extensive list of available extended permissions refer to 
// https://developers.facebook.com/docs/reference/api/permissions/
private string [] ExtendedPermissions = new [] { "user_about_me", "read_stream"};

FBLoginView loginView;
FBProfilePictureView pictureView;
FBGraphUser user;
UILabel nameLabel;

public override void ViewDidLoad ()
{
	base.ViewDidLoad ();

	// Create the Facebook LogIn View with the needed Permissions
	// https://developers.facebook.com/ios/login-ui-control/
	loginView = new FBLoginView (ExtendedPermissions) {
		Frame = new RectangleF (85, 20, 151, 43)
	};

	// Hook up to FetchedUserInfo event, so you know when
	// you have the user information available
	loginView.FetchedUserInfo += (sender, e) => {
		user = e.User;
		pictureView.ProfileID = user.Id;
		nameLabel.Text = user.Name;
	};

	// Clean user Picture and label when Logged Out
	loginView.ShowingLoggedOutUser += (sender, e) => {
		pictureView.ProfileID = null;
		nameLabel.Text = string.Empty;
	};

	// Create view that will display user's profile picture
	// https://developers.facebook.com/ios/profilepicture-ui-control/
	pictureView = new FBProfilePictureView () {
		Frame = new RectangleF (40, 71, 240, 240)
	};

	// Create the label that will hold user's facebook name
	nameLabel = new UILabel (new RectangleF (20, 319, 280, 21)) {
		TextAlignment = UITextAlignment.Center,
		BackgroundColor = UIColor.Clear
	};

	// Add views to main view
	View.AddSubview (loginView);
	View.AddSubview (pictureView);
	View.AddSubview (nameLabel);
}

```


## Documentation

* SDK Reference: [https://developers.facebook.com/docs/reference/ios/3.5/](https://developers.facebook.com/docs/reference/ios/3.5/)
* Conceptual Overview: [https://developers.facebook.com/concepts/ios-build-distribute-promote/](https://developers.facebook.com/concepts/ios-build-distribute-promote/)

## Contact & Discuss

* Bugs Tracker: [https://developers.facebook.com/bugs](https://developers.facebook.com/bugs)
* StackOverflow: [http://facebook.stackoverflow.com/questions/tagged/facebook-ios-sdk](http://facebook.stackoverflow.com/questions/tagged/facebook-ios-sdk)
