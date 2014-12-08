using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Windows.Input;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Bindings;
using Cirrious.MvvmCross.Binding.ExtensionMethods;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Views;

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

        public override int RowsInSection(UITableView tableview, int section)
        {
            if (ItemsSource == null)
            {
                return 0;
            }
            var itemCount = ItemsSource.Count();

            return itemCount;
        }     

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
