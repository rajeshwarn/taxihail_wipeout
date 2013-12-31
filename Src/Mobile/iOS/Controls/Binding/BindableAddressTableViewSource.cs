using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
	public class BindableAddressTableViewSource : BindableCommandTableViewSource
	{
		private UITableView _tableView;
		public BindableAddressTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
			base( tableView, cellStyle, identifier, bindingText, accessory )
		{
			_tableView = tableView;
		}
        	
//
//		public override int NumberOfSections (UITableView tableView)
//		{
//            var nb = 0;
//			var dataSource =  ((BindableAddressTableViewSource)tableView.Source).ItemsSource as IEnumerable<SectionAddressViewModel>;
//            if(dataSource != null)
//			    nb =  dataSource.Where( ds => (ds.Addresses != null) && ds.Addresses.Count() > 0 ).Count();
//            return nb;
//		}
//
//		public override int RowsInSection (UITableView tableview, int section)
//		{
//			var dataSource =  ((BindableAddressTableViewSource)tableview.Source).ItemsSource as IEnumerable<SectionAddressViewModel>;
//            return dataSource.ElementAt( section ).Addresses.Count();
//		}
//
//		protected override object GetItemAt (NSIndexPath indexPath)
//		{
//			var dataSource = ((BindableAddressTableViewSource)_tableView.Source).ItemsSource as IEnumerable<SectionAddressViewModel>;
//			return dataSource.ElementAt(indexPath.Section).Addresses.ElementAt(indexPath.Row);
//
//		}
//
//		public override string TitleForHeader (UITableView tableView, int section)
//		{
//			var dataSource = ((BindableAddressTableViewSource)tableView.Source).ItemsSource as IEnumerable<SectionAddressViewModel>;
//			return dataSource.ElementAt(section).SectionTitle;
//		}

	}
}

