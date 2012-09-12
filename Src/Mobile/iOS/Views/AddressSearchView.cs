using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Touch.Interfaces;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using System.Linq;
using apcurium.MK.Booking.Mobile.ListViewStructure;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class AddressSearchView : MvxBindingTouchViewController<AddressSearchViewModel>
	{
		private const string CELLID = "AdressCell";

		public AddressSearchView (MvxShowViewModelRequest request) : base (request)
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

			((TextField)SearchTextField).SetImage( "Assets/Search/SearchIcon.png" );
			SearchTextField.Placeholder = Resources.SearchPlaceholder;

			AppButtons.FormatStandardButton( (GradientButton)CancelButton, Resources.CancelBoutton, AppStyle.ButtonColor.Silver );

			// Perform any additional setup after loading the view, typically from a nib.

			var source = new MvxActionBasedBindableTableViewSource(
                                AddressListView, 
                                UITableViewCellStyle.Subtitle,
                                new NSString(CELLID), 
                                "test",
								UITableViewCellAccessory.None);
			
            source.CellCreator = (tview , iPath, state ) =>
            {
                return new TwoLinesAddressCell( ViewModel.DataSourceStructure.Sections.ElementAt(iPath.Section).Items.ElementAt(iPath.Row) as TwoLinesAddressItem, CELLID );
            };

			source.CellModifier = (cell) =>
				{
					
				};
			
            this.AddBindings(new Dictionary<object, string>()
		                         {
		                             {source, "{'ItemsSource':{'Path':'AddressList'}}"} ,
		                         });

            AddressListView.Source = source;


		}

		private void SearchOnClick()
		{

		}
		private void FavoritesOnClick()
		{
			ViewModel.GetFavoritesCommand.Execute();
		}

		private void ContactsOnClick()
		{
			ViewModel.GetContactsCommand.Execute();
		}

		private void PlacesOnClick()
		{
			ViewModel.GetPlacesCommand.Execute();
		}

		public AddressSearchViewModel ViewModel
        {
            get;
            set;
        }

        public bool IsVisible { get {return true ;} }

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

