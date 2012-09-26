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
	public class BindableAddressTableViewSource : MvxActionBasedBindableTableViewSource
	{
		private UITableView _tableView;
		public BindableAddressTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
			base( tableView, cellStyle, identifier, bindingText, accessory )
		{
			_tableView = tableView;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			base.RowSelected (tableView, indexPath);
			if ( SelectedCommand != null && SelectedCommand.CanExecute())
			{
				SelectedCommand.Execute( GetItemAt( indexPath ) as AddressViewModel );
			}
		}

		public override int NumberOfSections (UITableView tableView)
		{
			var dataSource =  ((BindableAddressTableViewSource)tableView.Source).ItemsSource as IEnumerable<SectionAddressViewModel>;
			return dataSource.Where( ds => (ds.Addresses != null) && ds.Addresses.Count() > 0 ).Count();
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			var dataSource =  ((BindableAddressTableViewSource)tableview.Source).ItemsSource as IEnumerable<SectionAddressViewModel>;
			return dataSource.ElementAt( section ).Addresses.Count();
		}

		protected override object GetItemAt (NSIndexPath indexPath)
		{
			var dataSource = ((BindableAddressTableViewSource)_tableView.Source).ItemsSource as IEnumerable<SectionAddressViewModel>;
			return dataSource.ElementAt(indexPath.Section).Addresses.ElementAt(indexPath.Row);

		}

		public override string TitleForHeader (UITableView tableView, int section)
		{
			var dataSource = ((BindableAddressTableViewSource)tableView.Source).ItemsSource as IEnumerable<SectionAddressViewModel>;
			return dataSource.ElementAt(section).SectionTitle;
		}

		public IMvxCommand SelectedCommand { get; set; }

	}
}

