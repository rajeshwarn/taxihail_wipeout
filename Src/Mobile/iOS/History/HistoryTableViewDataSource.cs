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
//			UITableViewCell cell = tableView.DequeueReusableCell (kCellIdentifier);
//			if (cell == null) {				
//				cell = new UITableViewCell (UITableViewCellStyle.Default , kCellIdentifier);
//			}
//			cell.BackgroundColor = UIColor.Clear;
//			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
//			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
//			cell.TextLabel.TextColor = UIColor.DarkGray;
//    		cell.TextLabel.Font = UIFont.SystemFontOfSize( 14 );
//			
//			var b = _list.ElementAt(indexPath.Row);
//			cell.TextLabel.Text = "#" + b.IBSOrderId + " - " + b.PickupAddress.FullAddress ;
//			return cell;

			TwoLinesAddressCell cell = (TwoLinesAddressCell)tableView.DequeueReusableCell (kCellIdentifier);
			if (cell == null) {				
				cell = new TwoLinesAddressCell ( _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ) as TwoLinesAddressItem, kCellIdentifier);
			}

			

//			cell.TextLabel.Text = "#" + b.IBSOrderId + " - " + b.PickupAddress.FullAddress ;
			return cell;
		}
		
	}
}


