
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;

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
				tableLocations.DataSource = new LocationTableViewDataSource (this, LocationList, new List<Address>(), Mode);
				tableLocations.Delegate = new LocationTableViewDelegate (this, LocationList, new List<Address>());
			}

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

		
		
		#endregion
	}
}

