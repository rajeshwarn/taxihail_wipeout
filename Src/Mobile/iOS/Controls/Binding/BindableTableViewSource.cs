using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
	public class BindableTableViewSource : BindableCommandTableViewSource
	{
        public BindableTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
			base( tableView, cellStyle, identifier, bindingText, accessory )
		{
		}

		public override float GetHeightForHeader(UITableView tableView, int section)
		{
			return 22;
		}

		public override UIView GetViewForHeader(UITableView tableView, int section)
		{
			return new UIView { BackgroundColor = UIColor.Clear };
		}

		public override float GetHeightForFooter(UITableView tableView, int section)
		{
			return 22;
		}

		public override UIView GetViewForFooter(UITableView tableView, int section)
		{
			return new UIView { BackgroundColor = UIColor.Clear };
		}
	}
}

