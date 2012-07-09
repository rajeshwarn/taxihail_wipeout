//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Text;
//
//using MonoTouch.Foundation;
//using MonoTouch.UIKit;
//namespace TaxiMobileApp
//{
//	public class LocationTableViewDelegate : UITableViewDelegate
//	{		
//
//		private List<LocationData> _favoriteList;
//		private List<LocationData> _historyList;
//		private LocationsTabView _parent;
//		public LocationTableViewDelegate (LocationsTabView parent, List<LocationData> favoriteList, List<LocationData> historyList)
//		{
//			_parent = parent;
//			_historyList = historyList;
//			_favoriteList = favoriteList;
//		}
//		  
//		public override void RowSelected (
//                UITableView tableView, NSIndexPath indexPath)
//            {
//			
//			var detail = new LocationDetailView();
//			if ( indexPath.Section == 0 )
//			{			
//				detail.LoadData( _favoriteList[indexPath.Row] );                
//			}
//			else{
//					detail.LoadData( _historyList[indexPath.Row] );                
//			}
//			_parent.NavigationController.PushViewController( detail, true  );
//			
//            }
//	}
//}
//
//
