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
	public class HistoryTableViewDataSource : UITableViewDataSource
	{

		static NSString kCellIdentifier = new NSString ("HistoryTableCellIdentifier");

		private IEnumerable<BookingInfoData> _list;

		public HistoryTableViewDataSource (IEnumerable<BookingInfoData> list)
		{
			_list = list;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _list.Count();
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (kCellIdentifier);
			if (cell == null) {				
				cell = new UITableViewCell (UITableViewCellStyle.Default, kCellIdentifier);
			}
			cell.BackgroundColor = UIColor.Clear;
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			cell.TextLabel.TextColor = UIColor.DarkGray;
    		cell.TextLabel.Font = UIFont.SystemFontOfSize( 14 );
			
			var b = _list.ElementAt(indexPath.Row);
			cell.TextLabel.Text = "#" + b.Id + " - " + b.PickupLocation.Address ;
			return cell;
		}
		
	}
}


