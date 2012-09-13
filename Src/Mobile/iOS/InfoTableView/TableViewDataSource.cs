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
using apcurium.MK.Booking.Mobile.ListViewStructure;


namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public class TableViewDataSource : UITableViewDataSource
	{

		static NSString kCellIdentifier = new NSString ("TableCellIdentifier");

		private InfoStructure _structure;

		public TableViewDataSource (InfoStructure structure)
		{
			_structure = structure;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _structure.Sections.ElementAt( section ).Items.Count();
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			SingleLineCell cell = (SingleLineCell)tableView.DequeueReusableCell (kCellIdentifier);
			if (cell == null) {				
				cell = new SingleLineCell ( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ) as SingleLineItem, kCellIdentifier);
			}
			else
			{
				((SingleLineCell)cell).ReUse( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ) as SingleLineItem );
			}

			return cell;
		}
		
	}
}


