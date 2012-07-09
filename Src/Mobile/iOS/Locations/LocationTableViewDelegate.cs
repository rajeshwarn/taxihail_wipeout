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
	public class LocationTableViewDelegate : UITableViewDelegate
	{

		private IEnumerable<LocationData> _favoriteList;
		private IEnumerable<LocationData> _historyList;
		private LocationsTabView _parent;
		public LocationTableViewDelegate (LocationsTabView parent, IEnumerable<LocationData> favoriteList, IEnumerable<LocationData> historyList)
		{
			_parent = parent;
			_historyList = historyList;
			_favoriteList = favoriteList;
		}

//		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
//		{
//			return 30;
//		}
//		

		private LocationData _lastSelected;

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			
			if ((indexPath.Section == 1) && _historyList.ElementAt (indexPath.Row).IsHistoricEmptyItem)
			{
				return;
			}
			
			if (_parent.Mode == LocationsTabViewMode.Edit)
			{
				var detail = new LocationDetailView ();
				_parent.NavigationController.PushViewController (detail, true);
				if (indexPath.Section == 0)
				{
					
					_lastSelected = _favoriteList.ElementAt (indexPath.Row);
					_lastSelected.IsNew = false;
					_lastSelected.IsFromHistory = false;
					if (_lastSelected.IsAddNewItem)
					{
						_lastSelected = new LocationData { IsNew = true ,Id=-1};						
					}
										
					
					detail.LoadData (_lastSelected);
					
				}

				else
				{
					_lastSelected = _historyList.ElementAt (indexPath.Row).Copy();
					_lastSelected.IsNew = true;
					_lastSelected.IsFromHistory = true;
					_lastSelected.Id = -1;
					detail.LoadData (_lastSelected);
				}
				detail.Deleted += HandleDetailDeleted;
				
				
				detail.Saved += HandleDetailSaved;
				
			}

			else if (_parent.Mode == LocationsTabViewMode.FavoritesSelector || _parent.Mode == LocationsTabViewMode.NearbyPlacesSelector )
			{
				if (indexPath.Section == 0)
				{
					
					_parent.DoSelect (_favoriteList.ElementAt (indexPath.Row));
					
					
				}

				else
				{
					_parent.DoSelect (_historyList.ElementAt (indexPath.Row));
				}
			}
			
			
			
			
			
		}

		void HandleDetailSaved (object sender, EventArgs e)
		{
			if(_lastSelected.IsNew) 
			{				
				_parent.AddNew (_lastSelected);				
			}

			else
			{
				_parent.Update (_lastSelected);
				
			}
			_lastSelected.IsNew = false;
			
		}


		void HandleDetailDeleted (object sender, EventArgs e)
		{
			if (!_lastSelected.IsNew)
			{
				_parent.Delete (_lastSelected);
			}
		}
		
	}
}


