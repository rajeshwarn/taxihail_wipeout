using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Windows.Input;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Bindings;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class BindableTableViewSource : MvxActionBasedTableViewSource
	{
        public BindableTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
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

        public ICommand SelectedCommand { get; set; }

		public override float GetHeightForHeader(UITableView tableView, int section)
		{
			// If header height is 1, that means we actually want 0
			return tableView.SectionHeaderHeight == 1 ? 0.000001f : tableView.SectionHeaderHeight;
		}

		public override UIView GetViewForHeader(UITableView tableView, int section)
		{
			return new UIView { BackgroundColor = UIColor.Clear };
		}

		public override float GetHeightForFooter(UITableView tableView, int section)
		{
			// If footer height is 1, that means we actually want 0
			return tableView.SectionFooterHeight == 1 ? 0.000001f : tableView.SectionFooterHeight;
		}

		public override UIView GetViewForFooter(UITableView tableView, int section)
		{
			return new UIView { BackgroundColor = UIColor.Clear };
		}
	}
}

