using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
namespace TaxiMobileApp
{
	public class LocationTableViewDataSource : UITableViewDataSource
	{
		static NSString kCellIdentifier = new NSString ("LocationTableCellIdentifier");

		private IEnumerable<LocationData> _favoriteList;
		private IEnumerable<LocationData> _historyList;
		private LocationsTabViewMode _mode;
		private LocationsTabView _parent;

		public LocationTableViewDataSource ( LocationsTabView parent, IEnumerable<LocationData> favoriteList, IEnumerable<LocationData> historyList, LocationsTabViewMode mode )
		{
			_parent = parent;
			_historyList = historyList;
			_favoriteList = favoriteList;
			_mode = mode;
		}


		public override int NumberOfSections (UITableView tableView)
		{
			return 2;
		}

		public override string TitleForHeader (UITableView tableView, int section)
		{
			if (section == 0 ) 
			{
				if( _mode == LocationsTabViewMode.Edit || _mode == LocationsTabViewMode.FavoritesSelector )
				{
					return Resources.FavoriteLocationsTitle;
				}
				else if( _mode == LocationsTabViewMode.NearbyPlacesSelector )
				{
					return Resources.NearbyPlacesTitle;
				}
			}
			else if( _mode == LocationsTabViewMode.Edit || _mode == LocationsTabViewMode.FavoritesSelector )
			{
				return Resources.LocationHistoryTitle;
			}

			return null;
		}




		public override int RowsInSection (UITableView tableview, int section)
		{
			if (section == 0) 
			{
				return _favoriteList.Count ();
			} 
			else 
			{
				return _historyList.Count ();
			}
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (kCellIdentifier);
			
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, kCellIdentifier);
			}
			
			cell.BackgroundColor = UIColor.Clear;
			
			if ( _parent.Mode == LocationsTabViewMode.Edit )
			{
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			}
			else{
				cell.Accessory = UITableViewCellAccessory.None;
			}
			cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			
			cell.TextLabel.TextColor = UIColor.DarkGray;
    		cell.TextLabel.Font = UIFont.SystemFontOfSize( 14 );
			
			if (indexPath.Section == 0) {
				var favLoc = _favoriteList.ElementAt (indexPath.Row);
				if (favLoc.IsAddNewItem) {
					cell.TextLabel.Text = Resources.LocationAddFavorite;
				} else {
					cell.TextLabel.Text = favLoc.Display;
				}
			} else {
				var hist = _historyList.ElementAt (indexPath.Row);
				if (hist.IsHistoricEmptyItem) {
					cell.TextLabel.Text = Resources.LocationNoHistory;
					cell.Accessory = UITableViewCellAccessory.None;
					cell.SelectionStyle = UITableViewCellSelectionStyle.None;
					
				} else {
					cell.TextLabel.Text = hist.Display;
				}
				
			}
			
			return cell;
		}
		
	}
}


