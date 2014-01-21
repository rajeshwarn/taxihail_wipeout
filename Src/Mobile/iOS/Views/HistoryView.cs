using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class HistoryView : BaseViewController<HistoryViewModel>
	{
		const string CellId = "HistoryCell";
		const string CellBindingText = @"
                   FirstLine Title;
                   SecondLine PickupAddress.DisplayAddress;
                   ShowRightArrow ShowRightArrow;
                   ShowPlusSign ShowPlusSign;
                   Icon Status, Converter=OrderStatusToImageNameConverter
				"; 

		public HistoryView () : base ("HistoryView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = Localize.GetValue ("View_History");
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
            View.BackgroundColor = UIColor.FromRGB (239, 239, 239);

			tableOrders.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
			tableOrders.BackgroundColor = UIColor.Clear;
			tableOrders.SeparatorColor = UIColor.Clear;

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

			var set = this.CreateBindingSet<HistoryView, HistoryViewModel> ();

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
			return cell;
		}
	}
}

