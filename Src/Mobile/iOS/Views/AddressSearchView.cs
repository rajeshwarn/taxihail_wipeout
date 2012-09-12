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
	public partial class AddressSearchView : MvxBindingTouchViewController<AddressSearchViewModel>, ITaxiViewController, ISelectableViewController, IRefreshableViewController
	{
		private const string CELLID = "AdressCell";

		const string CellBindingText = @"
                {
                   'TitleText':{'Path':'FriendlyName'},
                   'DetailText':{'Path':'FullAddress'},
                }";

        public AddressSearchView() 
            : base(new MvxShowViewModelRequest<AddressSearchViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }

        protected AddressSearchView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        protected AddressSearchView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
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
			this.NavigationController.NavigationBar.Hidden=true;


			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

			TopBar.AddButton( Resources.SearchButton, SearchOnClick );
			TopBar.AddButton( Resources.FavoritesButton, FavoritesOnClick );
			TopBar.AddButton( Resources.ContactsButton, ContactsOnClick );
			TopBar.AddButton( Resources.PlacesButton, PlacesOnClick );
			TopBar.SetSelected( 0 );

			((TextField)SearchTextField).SetImage( "Assets/Search/SearchIcon.png" );
			SearchTextField.Placeholder = Resources.SearchPlaceholder;

			AppButtons.FormatStandardButton( (GradientButton)CancelButton, Resources.CancelBoutton, AppStyle.ButtonColor.Silver );

			var source = new MvxActionBasedBindableTableViewSource(
                                AddressListView, 
                                UITableViewCellStyle.Subtitle,
                                new NSString(CELLID), 
                                CellBindingText,
								UITableViewCellAccessory.None);
			
            source.CellCreator = (tview , iPath, state ) =>
            {
                return new TwoLinesCell( CELLID, CellBindingText );
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

		public string GetTitle()
        {
            return "";
        }
        
      	public bool IsTopView
        {
            get { return this.NavigationController.TopViewController is AddressSearchView; }
        }

        public UIView GetTopView()
        {
            return null;
        }

        public void Selected()
        {
            try
            {


            }
            catch
            {
            }
        }

        public void RefreshData()
        {
            Selected();
        }
	}
}

