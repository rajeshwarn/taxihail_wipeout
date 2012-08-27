using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;


namespace apcurium.MK.Booking.Mobile.Client
{
	public class HistoryTableViewDataSource : UITableViewDataSource
	{

		static NSString kCellIdentifier = new NSString ("HistoryTableCellIdentifier");

		private InfoStructure _structure;

		public HistoryTableViewDataSource (InfoStructure structure)
		{
			_structure = structure;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _structure.Sections.ElementAt( section ).Items.Count();
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			TwoLinesAddressCell cell = (TwoLinesAddressCell)tableView.DequeueReusableCell (kCellIdentifier);
			if (cell == null) {				
				cell = new TwoLinesAddressCell ( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ) as TwoLinesAddressItem, kCellIdentifier);
			}
			else
			{
				((TwoLinesAddressCell)cell).ReUse( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ) as TwoLinesAddressItem );
			}

			return cell;
		}
		
	}
}


