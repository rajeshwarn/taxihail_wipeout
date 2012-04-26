using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace TaxiMobileApp
{
	public class LocationTableViewDataSource : UITableViewDataSource
	{

		static NSString kCellIdentifier = new NSString ("LocationTableCellIdentifier");

		private IEnumerable<LocationData> _favoriteList;
		private IEnumerable<LocationData> _historyList;
		private LocationsTabView _parent;
		public LocationTableViewDataSource ( LocationsTabView parent, IEnumerable<LocationData> favoriteList, IEnumerable<LocationData> historyList)
		{
			_parent = parent;
			_historyList = historyList;
			_favoriteList = favoriteList;
		}


		public override int NumberOfSections (UITableView tableView)
		{
			return 2;
		}

		public override string TitleForHeader (UITableView tableView, int section)
		{
			
			if (section == 0) {
				return Resources.FavoriteLocationsTitle;
			} else {
				return Resources.LocationHistoryTitle;
			}
		}




		public override int RowsInSection (UITableView tableview, int section)
		{
			if (section == 0) {
				return _favoriteList.Count ();
			} else {
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


