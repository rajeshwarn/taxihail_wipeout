using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[MvxViewFor(typeof(HistoryViewModel))]
	public partial class HistoryTabView : BaseViewController<HistoryViewModel>
	{

		private const string Cellid = "HistoryCell";
		
		static readonly string CellBindingText = @"
					FirstLine Title;
					FirstLineTextColor Status, Converter OrderStatusToTextColorConverter;
					SecondLine PickupAddress.DisplayAddress;
					ShowRightArrow ShowRightArrow;
					ShowPlusSign ShowPlusSign;
					IsFirst IsFirst;
					IsLast IsLast
				";
			
		public HistoryTabView() 
			: base("HistoryTabView", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
            NavigationItem.TitleView = new TitleView(null, Localize.GetValue("View_HistoryList"), true);

            lblInfo.Text = Localize.GetValue("HistoryInfo");	
			lblInfo.TextColor = AppStyle.TitleTextColor;
            lblNoHistory.Text = Localize.GetValue("NoHistoryLabel");
            lblNoHistory.Hidden = true;
            tableHistory.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableHistory.BackgroundColor = UIColor.Clear;

            var source = new BindableCommandTableViewSource(
				tableHistory, 
				UITableViewCellStyle.Subtitle,
				new NSString(Cellid), 
				CellBindingText,
				UITableViewCellAccessory.None);
			
			source.CellCreator = (tview , iPath, state ) => { 
                return new TwoLinesCell( Cellid, CellBindingText ); 
            };

			var set = this.CreateBindingSet<HistoryTabView, HistoryViewModel> ();

			set.Bind(tableHistory)
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

			tableHistory.Source = source;

            View.ApplyAppFont ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = false;
		}
	}
}

