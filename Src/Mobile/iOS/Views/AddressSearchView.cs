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
                   'FirstLine':{'Path':'Address.FriendlyName'},
                   'SecondLine':{'Path':'Address.FullAddress'},
				   'ShowRightArrow':{'Path':'ShowRightArrow'},
				   'ShowPlusSign':{'Path':'ShowPlusSign'},
				   'IsFirst':{'Path':'IsFirst'},
				   'IsLast':{'Path':'IsLast'},
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

			NavigationController.NavigationBar.Hidden=true;

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

			var searchBtn = TopBar.AddButton( Resources.SearchButton );
			var favoritesBtn = TopBar.AddButton( Resources.FavoritesButton );
			var contactsBtn = TopBar.AddButton( Resources.ContactsButton );
			var placesBtn = TopBar.AddButton( Resources.PlacesButton );
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
			
			source.CellCreator = (tview , iPath, state ) => { return new TwoLinesCell( CELLID, CellBindingText ); };

            this.AddBindings(new Dictionary<object, string>(){
				{CancelButton, "{'TouchUpInside':{'Path':'CloseViewCommand'}}"},
				{source, "{'ItemsSource':{'Path':'AddressViewModels'}}"} ,
				{favoritesBtn, "{'TouchUpInside':{'Path':'GetFavoritesCommand'}}"} ,
				{contactsBtn, "{'TouchUpInside':{'Path':'GetContactsCommand'}}"} ,
				{placesBtn, "{'TouchUpInside':{'Path':'GetPlacesCommand'}}"} ,
				{searchBtn, "{'TouchUpInside':{'Path':'ResetCommand'}}"} ,
				{SearchTextField, "{'Text':{'Path':'SearchText'}}"} ,

			});

            AddressListView.Source = source;

            SearchTextField.ReturnKeyType = UIReturnKeyType.Done;
            SearchTextField.ShouldReturn = delegate(UITextField textField)
            {
				return SearchTextField.ResignFirstResponder();
            };
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

