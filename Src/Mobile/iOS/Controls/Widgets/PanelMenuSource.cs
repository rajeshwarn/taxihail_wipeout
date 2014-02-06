using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class PanelMenuSource : MvxActionBasedTableViewSource
    {
        private NSString _cellId;
        private string _cellBindingText;

		public PanelMenuSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
		base( tableView, cellStyle, identifier, bindingText, accessory )
		{
            _cellId = identifier;
            _cellBindingText = bindingText;
		}

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
        {
            PanelMenuCell result = tableView.DequeueReusableCell (this.CellIdentifier) as PanelMenuCell;

            if (result == null)
            {
                result = new PanelMenuCell(_cellId, _cellBindingText); 
            }

            result.HideBottomBar = tableView.IsLastCell(indexPath);
            result.RemoveDelay();
            return result as UITableViewCell;
        }
       
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			base.RowSelected (tableView, indexPath);
			tableView.DeselectRow(indexPath, true);
		}
    }
}

