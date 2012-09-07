
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
	public enum LocationsTabViewMode
	{
		Edit,
		FavoritesSelector,
		NearbyPlacesSelector
	}
	public partial class LocationsTabView : UIViewController , ITaxiViewController, ISelectableViewController,IRefreshableViewController
	{
//		private TableView _tableLocations;
		public event EventHandler Canceled;
		public event EventHandler LocationSelected;
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public LocationsTabView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public LocationsTabView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public LocationsTabView () : base("LocationsTabView", null)
		{
			Mode = LocationsTabViewMode.Edit;
			Initialize ();
		}



		void Initialize ()
		{

		}
		
		public UIView GetTopView()
		{
			return null;
		}
		
		public string GetTitle()
		{
			return  Resources.LocationViewTitle;
		}
		
		public LocationsTabViewMode Mode { get; set; }


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
//			_tableLocations = new TableView( new System.Drawing.RectangleF(0,-2,320,366), UITableViewStyle.Grouped );
//			_tableLocations.SectionFooterHeight = 10;
//			_tableLocations.SectionHeaderHeight = 10;
//			_tableLocations.BackgroundColor = UIColor.Clear;
//			View.AddSubview( _tableLocations );
//			tableLocations.Hidden = true;

			if (Mode == LocationsTabViewMode.Edit) {
				View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
								
				
			} else {
				//View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background_full.png"));
				View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
				NavigationItem.RightBarButtonItem = new UIBarButtonItem( UIBarButtonSystemItem.Cancel , CanceledTouchUpInside );
				//btnCancel.Hidden = false;
				//btnCancel.TouchUpInside += CanceledTouchUpInside;
			}
//			btnCancel.SetTitle (Resources.CancelBoutton, UIControlState.Normal);
//			lblTitle.Text = "";

			tableLocations.SectionHeaderHeight = 33;
			Layout();
			LoadGridData ();


			
		}

		void CanceledTouchUpInside (object sender, EventArgs e)
		{
			if (Canceled != null) {
				Canceled (this, EventArgs.Empty);
			}
		}
		
		
		public void RefreshData()
		{
			LoadGridData ();
		}
		
			
		public void Selected ()
		{
			LoadGridData ();
		}

		private void Layout()
		{

		}

		private void LoadGridData ()
		{
			if (tableLocations == null) {
				return;
			}

			var structure = GetLocationsStructure();

			tableLocations.DataSource = new LocationTableViewDataSource (this, structure);
			tableLocations.Delegate = new LocationTableViewDelegate (this, structure);

			tableLocations.ReloadData ();
		}

		public List<Address> LocationList { get; set; }

		private List<Address> GetFavorites()
		{
			List<Address> favorites = new List<Address>();
            var adrs =TinyIoCContainer.Current.Resolve<IAccountService>().GetFavoriteAddresses();
            if ( adrs.Count() > 0 )
            {
                favorites.AddRange( adrs );
            }
			
			if ( Mode == LocationsTabViewMode.Edit )
			{
				favorites.Add (new Address());
			}
			return favorites;
		}

		private List<Address> GetHistoric()
		{
			List<Address> historics = new List<Address>();
            var adrs =TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryAddresses();
            if ( adrs.Count() > 0 )
            {
                historics.AddRange( adrs );
            }
            return historics;
		}


		public void DoSelect ( Address data )
		{
			SelectedLocation =data;
			if ( LocationSelected != null )
			{
				LocationSelected( this, EventArgs.Empty );
			}
		}

		public Address SelectedLocation { get; set; }

		public void Delete (Address data)
		{						
            TinyIoCContainer.Current.Resolve<IAccountService>().DeleteAddress( data.Id );
			LoadGridData ();
		}

		public void Update (Address data)
		{
			TinyIoCContainer.Current.Resolve<IAccountService>().UpdateAddress( data );           
            LoadGridData ();
		}

		
		private InfoStructure GetLocationsStructure()
		{
			var structure = new InfoStructure( 44, false );

			if( Mode == LocationsTabViewMode.Edit || Mode == LocationsTabViewMode.FavoritesSelector )
			{
				var favorites = GetFavorites();
				var historic = GetHistoric();

				var sectFav = structure.AddSection( Resources.FavoriteLocationsTitle );

				sectFav.SectionLabelTextColor = AppStyle.TitleTextColor.ToArray();
				sectFav.EditMode = Mode == LocationsTabViewMode.Edit;
				favorites.ForEach( item => sectFav.AddItem( new TwoLinesAddressItem( item.Id,  item.Id.IsNullOrEmpty() ? Resources.LocationAddFavorite : item.FriendlyName, item.Id.IsNullOrEmpty() ? Resources.LocationAddFavoriteDetails : item.FullAddress ) { Data = item, ShowRightArrow = Mode == LocationsTabViewMode.Edit && !item.Id.IsNullOrEmpty(), ShowPlusSign = item.Id.IsNullOrEmpty() } ) );

				var sectHist = structure.AddSection( Resources.LocationHistoryTitle );
				sectHist.SectionLabelTextColor = AppStyle.TitleTextColor.ToArray();
				sectHist.EditMode = Mode == LocationsTabViewMode.Edit;
				historic.ForEach( item => sectHist.AddItem( new TwoLinesAddressItem( item.Id,  item.Id.IsNullOrEmpty() ? Resources.LocationNoHistory : item.FriendlyName, item.Id.IsNullOrEmpty() ? Resources.LocationNoHistoryDetails : item.FullAddress ) { Data = item, ShowRightArrow = Mode == LocationsTabViewMode.Edit && !item.Id.IsNullOrEmpty(), Enabled = () => !item.Id.IsNullOrEmpty() } ) );
			}
			else if( Mode == LocationsTabViewMode.NearbyPlacesSelector )
			{
				var sectNearby = structure.AddSection( Resources.NearbyPlacesTitle );
				sectNearby.EditMode = false;
				LocationList.ForEach( item => sectNearby.AddItem( new TwoLinesAddressItem( item.Id,  item.FriendlyName, item.FullAddress ) { Data = item } ) );
			}

			return structure;
		}
		#endregion
	}
}

