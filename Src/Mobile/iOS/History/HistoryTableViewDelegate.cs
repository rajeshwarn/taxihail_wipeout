using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;


namespace apcurium.MK.Booking.Mobile.Client
{
	public class HistoryTableViewDelegate : UITableViewDelegate
	{
		private IEnumerable<Order> _list;
		private HistoryTabView _parent;
		
		public HistoryTableViewDelegate (HistoryTabView parent, IEnumerable<Order> list)
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


