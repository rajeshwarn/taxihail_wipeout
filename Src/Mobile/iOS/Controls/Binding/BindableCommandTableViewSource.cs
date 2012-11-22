using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Cirrious.MvvmCross.Interfaces.Commands;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
	public class BindableCommandTableViewSource : MvxActionBasedBindableTableViewSource
	{
        public BindableCommandTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
			base( tableView, cellStyle, identifier, bindingText, accessory )
		{
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			base.RowSelected (tableView, indexPath);
			if ( SelectedCommand != null && SelectedCommand.CanExecute())
			{
				SelectedCommand.Execute(GetItemAt( indexPath ));
			}
		}

		public IMvxCommand SelectedCommand { get; set; }

	}
}

