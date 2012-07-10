
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;

namespace TaxiMobileApp
{
	public enum LocationsTabViewMode
	{
		Edit,
		FavoritesSelector,
		NearbyPlacesSelector
	}
	public partial class LocationsTabView : UIViewController , ITaxiViewController, ISelectableViewController,IRefreshableViewController
	{
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
			tableLocations.RowHeight = 35;
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

			if( Mode == LocationsTabViewMode.Edit || Mode == LocationsTabViewMode.FavoritesSelector )
			{
				var favorites = GetFavorites();
				var historic = GetHistoric();
				tableLocations.DataSource = new LocationTableViewDataSource (this,favorites, historic, Mode);
				tableLocations.Delegate = new LocationTableViewDelegate (this, favorites, historic);
			}
			else if( Mode == LocationsTabViewMode.NearbyPlacesSelector )
			{
				tableLocations.DataSource = new LocationTableViewDataSource (this, LocationList, new List<LocationData>(), Mode);
				tableLocations.Delegate = new LocationTableViewDelegate (this, LocationList, new List<LocationData>());
			}

			tableLocations.ReloadData ();
		}

		public List<LocationData> LocationList { get; set; }

		private List<LocationData> GetFavorites()
		{
			List<LocationData> favorites = new List<LocationData>();
			if  ( AppContext.Current.LoggedUser.FavoriteLocations.Count () > 0) {
				favorites.AddRange (AppContext.Current.LoggedUser.FavoriteLocations );
			}
			
			if ( Mode == LocationsTabViewMode.Edit )
			{
				favorites.Add (new LocationData { IsAddNewItem = true });
			}
			return favorites;
		}

		private List<LocationData> GetHistoric()
		{
			List<LocationData> historics = new List<LocationData>();
			var historicList = AppContext.Current.LoggedUser.BookingHistory.Where (b => !b.Hide && b.PickupLocation.Name.IsNullOrEmpty ())				
				.OrderByDescending (b => b.RequestedDateTime)
				.Select (b => b.PickupLocation).ToArray ();
			
			if ( historicList.Count() > 0 )
			{
				historics.AddRange(historicList);
			}
			    
			var historic = historics.Where( h=>h.Address.HasValue()).GroupBy( l=>l.Address + "_" + l.Apartment.ToSafeString() + "_" + l.RingCode.ToSafeString() ).Select( g=>g.ElementAt(0)).ToArray();
			
			if (historic.Count () == 0) {
				historic = new LocationData[1];
				historic[0] = new LocationData { IsHistoricEmptyItem = true };
			}
			
			historic.ForEach( h => h.IsFromHistory =true);

			return historic.ToList();
		}


		public void DoSelect ( LocationData data )
		{
			SelectedLocation =data;
			if ( LocationSelected != null )
			{
				LocationSelected( this, EventArgs.Empty );
			}
		}

		public LocationData SelectedLocation { get; set; }

		public void Delete (LocationData data)
		{
			var newList = new List<LocationData> ();
			
			
			if ((AppContext.Current.LoggedUser.FavoriteLocations != null) && (AppContext.Current.LoggedUser.FavoriteLocations.Count () > 0)) {
				newList.AddRange (AppContext.Current.LoggedUser.FavoriteLocations);
			}
			newList.Remove (data);
			AppContext.Current.LoggedUser.FavoriteLocations = newList.ToArray ();
			
			
			
			if (AppContext.Current.LoggedUser.BookingHistory != null) {
				AppContext.Current.LoggedUser.BookingHistory.FirstOrDefault (b => b.PickupLocation == data).Maybe (b => b.Hide = true);				
			}
			
			AppContext.Current.UpdateLoggedInUser (AppContext.Current.LoggedUser,true);
			LoadGridData ();
		}

		public void Update (LocationData data)
		{
			AppContext.Current.UpdateLoggedInUser (AppContext.Current.LoggedUser,true);
			LoadGridData ();
		}

		public void AddNew (LocationData data)
		{
			Console.WriteLine (data.Address + "-" + data.Name);
			var newList = new List<LocationData> ();
			if ((AppContext.Current.LoggedUser.FavoriteLocations != null) && (AppContext.Current.LoggedUser.FavoriteLocations.Count () > 0)) {
				newList.AddRange (AppContext.Current.LoggedUser.FavoriteLocations);
			}
			newList.Add (data);
			AppContext.Current.LoggedUser.FavoriteLocations = newList.ToArray ();
			AppContext.Current.UpdateLoggedInUser (AppContext.Current.LoggedUser,true);
			LoadGridData ();
		}
		
		#endregion
	}
}

