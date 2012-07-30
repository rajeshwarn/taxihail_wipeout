//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Text;
//
//using MonoTouch.Foundation;
//using MonoTouch.UIKit;
//namespace apcurium.MK.Booking.Mobile.Client
//{
//	public class LocationTableViewDataSource : UITableViewDataSource
//	{
//
//		static NSString kCellIdentifier = new NSString ("LocationTableCellIdentifier");
//
//		private List<Address> _favoriteList;
//		private List<Address> _historyList;
//
//		public LocationTableViewDataSource (List<Address> favoriteList, List<Address> historyList)
//		{
//			_historyList = historyList;
//			_favoriteList = favoriteList;
//		}
//		
//		
//		public override int NumberOfSections (UITableView tableView)
//		{
//			return 2;
//		}
//		
//		public override string TitleForHeader (UITableView tableView, int section)
//		{
//		
//			if (section == 0 )
//			{
//				return Resources.FavoriteLocationsTitle;
//			}
//			else{				
//				return  Resources.LocationHistoryTitle;
//			}
//		}
//		
//		
//		
//		public override int RowsInSection (UITableView tableview, int section)
//		{
//			if ( section == 0 )
//			{
//				return _favoriteList.Count;
//			}
//			else{
//				return _historyList.Count;
//			}
//			
//		}
//		
//		
//		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
//		{
//			UITableViewCell cell = tableView.DequeueReusableCell (kCellIdentifier);				
//			
//			if (cell == null) {
//				cell = new UITableViewCell (UITableViewCellStyle.Default , kCellIdentifier);
//			}
//			
//			cell.BackgroundColor = UIColor.Clear;
//			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
//			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
//			
//			if ( indexPath.Section == 0 )
//			{
//			cell.TextLabel.Text = _favoriteList[indexPath.Row].Display;
//			}
//			else{
//				cell.TextLabel.Text = _historyList[indexPath.Row].Display;
//			
//			}
//			
//			return cell;
//		}
//		
//	}
//}
//
//
