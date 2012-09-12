using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.ListViewStructure;


namespace apcurium.MK.Booking.Mobile.Client
{
	public class HistoryTableViewDelegate : UITableViewDelegate
	{
		private InfoStructure _structure;
		private HistoryTabView _parent;
		
		public HistoryTableViewDelegate (HistoryTabView parent, InfoStructure structure)
		{
			_parent = parent;
			_structure = structure;
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ).RowHeight;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			var detail = new HistoryDetailView (_parent);
			
			detail.LoadData ( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ).Data as Order );
							
			_parent.NavigationController.PushViewController (detail, true);
			
		}
		
	}
}


