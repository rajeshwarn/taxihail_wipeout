using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
	public class BindableAddressTableViewSource : MvxActionBasedBindableTableViewSource
	{
		public BindableAddressTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
			base( tableView, cellStyle, identifier, bindingText, accessory )
		{

		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			base.RowSelected (tableView, indexPath);
			if ( SelectedCommand != null && SelectedCommand.CanExecute())
			{
				SelectedCommand.Execute( GetItemAt( indexPath ) as AddressViewModel );
			}
		}

		public IMvxCommand SelectedCommand { get; set; }
	}
}

