using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class HistoryListView : BaseViewController<HistoryListViewModel>
	{
		const string CellId = "HistoryCell";
		const string CellBindingText = @"
                   FirstLine Title;
                   SecondLine PickupAddress.DisplayAddress;
                   ShowRightArrow ShowRightArrow;
                   ShowPlusSign ShowPlusSign;
                   Icon Status, Converter=OrderStatusToImageNameConverter
				"; 

        public HistoryListView () : base ("HistoryListView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = Localize.GetValue ("View_History");

            ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

			tableOrders.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
			tableOrders.BackgroundColor = UIColor.Clear;
			tableOrders.SeparatorColor = UIColor.Clear;
            tableOrders.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableOrders.DelaysContentTouches = false;

			lblNoHistory.Text = Localize.GetValue("HistoryViewNoHistoryLabel");
			lblNoHistory.Hidden = true;

            var source = new BindableTableViewSource (
				tableOrders, 
				UITableViewCellStyle.Subtitle, 
				new NSString (CellId), 
				CellBindingText, 
				UITableViewCellAccessory.None
			);
			source.CellCreator = CellCreator;
			tableOrders.Source = source;

            var set = this.CreateBindingSet<HistoryListView, HistoryListViewModel> ();

			set.Bind(tableOrders)
				.For(v => v.Hidden)
				.To(vm => vm.HasOrders)
				.WithConversion("BoolInverter");

			set.Bind(lblNoHistory)
				.For(v => v.Hidden)
				.To(vm => vm.HasOrders);

			set.Bind(source)
				.For(v => v.ItemsSource)
				.To(vm => vm.Orders);
			set.Bind(source)
				.For(v => v.SelectedCommand)
				.To(vm => vm.NavigateToHistoryDetailPage);

			set.Apply ();
		}

		private MvxStandardTableViewCell CellCreator(UITableView tableView, NSIndexPath indexPath, object state)
		{
			var cell = new TwoLinesCell(new NSString(CellId), CellBindingText, UITableViewCellAccessory.Checkmark);
			cell.HideBottomBar = tableView.IsLastCell(indexPath);
            cell.RemoveDelay();
			return cell;
		}
	}
}

