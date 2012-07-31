using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class LocationTableViewDelegate : UITableViewDelegate
	{

		private IEnumerable<Address> _favoriteList;
		private IEnumerable<Address> _historyList;
		private LocationsTabView _parent;
		public LocationTableViewDelegate (LocationsTabView parent, IEnumerable<Address> favoriteList, IEnumerable<Address> historyList)
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

		private Address _lastSelected;

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			
			if ((indexPath.Section == 1) && _historyList.ElementAt (indexPath.Row).Id.IsNullOrEmpty())
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
					detail.LoadData (_lastSelected);
					
				}

				else
				{
					_lastSelected = _historyList.ElementAt (indexPath.Row).Copy();					
                    _lastSelected.Id = Guid.Empty;
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
    

            _parent.Update (_lastSelected);             

			
		}


		void HandleDetailDeleted (object sender, EventArgs e)
		{
			_parent.Delete (_lastSelected);
		}
		
	}
}


