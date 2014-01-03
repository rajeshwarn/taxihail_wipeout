using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
	public class BindableAddressTableViewSource : BindableCommandTableViewSource
	{
		public BindableAddressTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
			base( tableView, cellStyle, identifier, bindingText, accessory )
		{
		}
	}
}

