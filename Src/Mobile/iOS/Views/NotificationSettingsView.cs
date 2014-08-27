using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class NotificationSettingsView : BaseViewController<NotificationSettingsViewModel>
	{
		const string CellId = "LocationCell";
		const string CellBindingText = @"
                   DisplayName Name;
				";

		public NotificationSettingsView () : base ("NotificationSettingsView", null)
		{
		}
			
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = Localize.GetValue ("View_NotificationSettings");

			ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

			tableNotifications.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
			tableNotifications.BackgroundColor = UIColor.Clear;
			tableNotifications.SeparatorColor = UIColor.Clear;
			tableNotifications.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			tableNotifications.DelaysContentTouches = false;

			var source = new BindableTableViewSource (
				tableNotifications, 
				UITableViewCellStyle.Subtitle, 
				new NSString (CellId), 
				CellBindingText, 
				UITableViewCellAccessory.None
			);
			source.CellCreator = CellCreator;
			tableNotifications.Source = source;

			var set = this.CreateBindingSet<NotificationSettingsView, NotificationSettingsViewModel> ();

			set.Bind(source)
				.For(v => v.ItemsSource)
				.To(vm => vm.NotificationSettings);
			/*set.Bind(source)
				.For(v => v.V)
				.To(vm => vm.NavigateToLocationDetailPage);*/

			set.Apply ();
		}

		private MvxStandardTableViewCell CellCreator(UITableView tableView, NSIndexPath indexPath, object state)
		{
			var cell = new ToggleCell(new NSString(CellId), CellBindingText, UITableViewCellAccessory.Checkmark);
			cell.HideBottomBar = tableView.IsLastCell(indexPath);
			cell.RemoveDelay();
			return cell;
		}
	}
}

