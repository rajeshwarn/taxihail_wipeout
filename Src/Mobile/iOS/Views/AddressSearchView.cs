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
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using MonoTouch.AddressBook;
using Xamarin.Contacts;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class AddressSearchView : MvxBindingTouchViewController<AddressSearchViewModel>, INavigationView
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
		#region Constructors
        public AddressSearchView() 
            : base(new MvxShowViewModelRequest<AddressSearchViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
        }

        public AddressSearchView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }

        public AddressSearchView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }	
		#endregion

        public bool HideNavigationBar
        {
            get { return true;}
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

			var searchBtn = TopBar.AddButton( Resources.TabSearch, "SearchBtn" );
			var favoritesBtn = TopBar.AddButton( Resources.TabFavorites, "FavoritesBtn" );
			var contactsBtn = TopBar.AddButton( Resources.TabContacts, "ContactsBtn" );
			var placesBtn = TopBar.AddButton( Resources.TabPlaces, "PlacesBtn" );
			TopBar.SetSelected( 0 );

			((SearchTextField)SearchTextField).SetImage( "Assets/Search/SearchIcon.png" );
			SearchTextField.Placeholder = Resources.SearchHint;

			AppButtons.FormatStandardButton( (GradientButton)CancelButton, Resources.CancelBoutton, AppStyle.ButtonColor.Silver );

			var source = new BindableAddressTableViewSource(
                                AddressListView, 
                                UITableViewCellStyle.Subtitle,
                                new NSString(CELLID), 
                                CellBindingText,
								UITableViewCellAccessory.None);

			source.CellCreator = (tview , iPath, state ) => { return new TwoLinesCell( CELLID, CellBindingText ); };

            this.AddBindings(new Dictionary<object, string>(){
				{CancelButton, "{'TouchUpInside':{'Path':'CloseViewCommand'}}"},
				{source, "{'ItemsSource':{'Path':'AllAddresses'}, 'SelectedCommand':{'Path':'RowSelectedCommand'}}"} ,
				{favoritesBtn, "{'SelectedChangedCommand':{'Path':'SelectedChangedCommand'}, 'Selected':{'Path':'FavoritesSelected'}}"} ,
				{contactsBtn, "{'SelectedChangedCommand':{'Path':'SelectedChangedCommand'}, 'Selected':{'Path':'ContactsSelected'}}"} ,
				{placesBtn, "{'SelectedChangedCommand':{'Path':'SelectedChangedCommand'}, 'Selected':{'Path':'PlacesSelected'}}"} ,
				{searchBtn, "{'SelectedChangedCommand':{'Path':'SelectedChangedCommand'}, 'Selected':{'Path':'SearchSelected'}}"} ,
                {SearchTextField, "{'Text':{'Path':'Criteria'}, 'TextChangedCommand':{'Path':'SearchCommand'}, 'IsProgressing':{'Path':'IsSearching'}}"} ,
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

