using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ListViewStructure;


namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public class TableViewDelegate : UITableViewDelegate
	{
		private readonly InfoStructure _structure;
		
		public TableViewDelegate ( InfoStructure structure)
		{
			_structure = structure;
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ).RowHeight;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			_structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ).OnItemSelected( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ) );
		}
		
	}
}


