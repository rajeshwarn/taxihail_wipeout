using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ListViewStructure;


namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public class TableViewDataSource : UITableViewDataSource
	{

		static readonly NSString KCellIdentifier = new NSString ("TableCellIdentifier");

		private readonly InfoStructure _structure;

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
			var cell = (SingleLineCell)tableView.DequeueReusableCell (KCellIdentifier);
			if (cell == null) {				
				cell = new SingleLineCell ( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ) as SingleLineItem, KCellIdentifier);
			}
			else
			{
				cell.ReUse( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ) as SingleLineItem );
			}

			return cell;
		}
		
	}
}


