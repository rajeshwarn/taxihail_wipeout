using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace TaxiMobileApp
{
	public class HistoryTableViewDelegate : UITableViewDelegate
	{
		private IEnumerable<BookingInfoData> _list;
		private HistoryTabView _parent;
		
		public HistoryTableViewDelegate (HistoryTabView parent, IEnumerable<BookingInfoData> list)
		{
			_parent = parent;
			_list = list;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			var detail = new HistoryDetailView (_parent);
			
			detail.LoadData (_list.ElementAt( indexPath.Row ) );
							
			_parent.NavigationController.PushViewController (detail, true);
			
		}
		
	}
}


