using System;
using UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Windows.Input;
using Cirrious.MvvmCross.Binding.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class AmpTableViewSource<T> : MvxSimpleTableViewSource
    {
        public AmpTableViewSource (UITableView tableView) 
            : base(tableView, typeof(T).Name, typeof(T).Name)
        {
            SelectedItemChanged += (sender, e) => 
            {
                ItemClickCommand.ExecuteIfPossible(SelectedItem);
            };
        }

        public ICommand ItemClickCommand { get; set; }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (ItemsSource == null)
            {
                return 0;
            }
            var itemCount = ItemsSource.Count();

            return itemCount;
        }     

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            // If header height is 1, that means we actually want 0
            return tableView.SectionHeaderHeight == 1 ? 0.000001f : tableView.SectionHeaderHeight;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            return new UIView { BackgroundColor = UIColor.Clear };
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            // If footer height is 1, that means we actually want 0
            return tableView.SectionFooterHeight == 1 ? 0.000001f : tableView.SectionFooterHeight;
        }

        public override UIView GetViewForFooter(UITableView tableView, nint section)
        {
            return new UIView { BackgroundColor = UIColor.Clear };
        }
    }
}
