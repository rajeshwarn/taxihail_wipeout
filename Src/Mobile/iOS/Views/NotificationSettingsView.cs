using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Localization;
using MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class NotificationSettingsView : BaseViewController<NotificationSettingsViewModel>
	{
		const string CellId = "NotificationCell";

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
				string.Empty, 
				UITableViewCellAccessory.None
			);
			source.CellCreator = CellCreator;
			tableNotifications.Source = source;

			labelNotificationEnabled.Text = this.Services().Localize["Notification_Enabled"];
			labelNotificationEnabled.TextColor = UIColor.FromRGB(44, 44, 44);
			labelNotificationEnabled.BackgroundColor = UIColor.Clear;
			labelNotificationEnabled.Font = UIFont.FromName(FontName.HelveticaNeueLight, 32 / 2);

			// Horizontal line bellow master notification toggle
			var enabledToggleSeparator = Line.CreateHorizontal(0, labelNotificationEnabled.Superview.Frame.Height, this.View.Frame.Width, UIColor.LightGray, 1f);
			enabledToggleSeparator.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			this.View.AddSubview(enabledToggleSeparator);

			var set = this.CreateBindingSet<NotificationSettingsView, NotificationSettingsViewModel> ();

			set.Bind(source)
				.For(v => v.ItemsSource)
				.To(vm => vm.NotificationSettings);
			set.Bind(switchNotificationEnabled)
				.For (v => v.On)
				.To (vm => vm.IsNotificationEnabled);
			set.Bind (tableNotifications)
				.For (v => v.Hidden)
				.To (vm => vm.IsNotificationEnabled)
				.WithConversion("BoolInverter");

			set.Apply ();
		}

		private MvxStandardTableViewCell CellCreator(UITableView tableView, NSIndexPath indexPath, object state)
		{
			var cell = new ToggleCell(new NSString(CellId), string.Empty, UITableViewCellAccessory.Checkmark);
			cell.HideBottomBar = tableView.IsLastCell(indexPath);
			cell.RemoveDelay();
			return cell;
		}
	}
}

