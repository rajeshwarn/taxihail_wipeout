
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class AddressSearchView : UIViewController
	{
		public AddressSearchView () : base ("AddressSearchView", null)
		{
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

			TopBar.AddButton( Resources.SearchButton, SearchOnClick );
			TopBar.AddButton( Resources.FavoritesButton, FavoritesOnClick );
			TopBar.AddButton( Resources.ContactsButton, ContactsOnClick );
			TopBar.AddButton( Resources.PlacesButton, PlacesOnClick );
			TopBar.SetSelected( 0 );

			// Perform any additional setup after loading the view, typically from a nib.
		}

		private void SearchOnClick()
		{}
		private void FavoritesOnClick()
		{}
		private void ContactsOnClick()
		{}
		private void PlacesOnClick()
		{}
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}

