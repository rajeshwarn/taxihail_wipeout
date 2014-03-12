using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
	public static class UITableViewExtensions
	{
		public static bool IsLastCell(this UITableView tableView, NSIndexPath indexPath)
		{
			var nbSections = tableView.NumberOfSections();
			var nbRows = tableView.NumberOfRowsInSection(indexPath.Section);
			bool isLastCell = indexPath.Section == nbSections - 1 && indexPath.Row == nbRows - 1;

			return isLastCell;
		}
	}
}

