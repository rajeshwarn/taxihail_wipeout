using System;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections.ObjectModel;
using System.Drawing;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Views.AddressPicker
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

        public ICommand SelectedCommand { get; set; }

    }

}
