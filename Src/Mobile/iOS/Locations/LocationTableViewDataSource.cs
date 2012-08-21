using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;


namespace apcurium.MK.Booking.Mobile.Client
{
	public class LocationTableViewDataSource : UITableViewDataSource
	{
		static NSString kCellIdentifier = new NSString ("LocationTableCellIdentifier");

		private InfoStructure _structure;
		private LocationsTabView _parent;

		public LocationTableViewDataSource ( LocationsTabView parent, InfoStructure structure )
		{
			_parent = parent;
			_structure = structure;
		}

		public override int NumberOfSections (UITableView tableView)
		{
			return _structure.Sections.Count();
		}

		public override string TitleForHeader (UITableView tableView, int section)
		{
			return _structure.Sections.ElementAt( section ).SectionLabel;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return _structure.Sections.ElementAt( section ).Items.Count();
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (kCellIdentifier);
			
			if (cell == null) {
				cell = new TwoLinesAddressCell ( (TwoLinesAddressItem)_structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ), kCellIdentifier);
			}
			
			return cell;
		}
		
	}
}


