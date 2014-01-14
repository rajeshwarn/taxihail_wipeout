using System.Windows.Input;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
	public class BindableCommandTableViewSource : MvxActionBasedTableViewSource
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

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public ICommand SelectedCommand { get; set; }
// ReSharper restore MemberCanBePrivate.Global

	}
}

