using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class UserTaxiHailNetworkSettingsView : BaseViewController<UserTaxiHailNetworkSettingsViewModel>
	{
		const string CellId = "FleetCell";

		public UserTaxiHailNetworkSettingsView () : base ("UserTaxiHailNetworkSettingsView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = Localize.GetValue ("View_UserTaxiHailNetworkSettings");

			ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

			tableTaxiHailNetworkSettings.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
			tableTaxiHailNetworkSettings.BackgroundColor = UIColor.Clear;
			tableTaxiHailNetworkSettings.SeparatorColor = UIColor.Clear;
			tableTaxiHailNetworkSettings.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			tableTaxiHailNetworkSettings.DelaysContentTouches = false;

			var source = new BindableTableViewSource (
				tableTaxiHailNetworkSettings, 
				UITableViewCellStyle.Subtitle,
				new NSString (CellId),  
				string.Empty, 
				UITableViewCellAccessory.None
			);
			source.CellCreator = CellCreator;
			tableTaxiHailNetworkSettings.Source = source;

			labelTaxiHailNetworkEnabled.Text = this.Services().Localize["Notification_Enabled"];
			labelTaxiHailNetworkEnabled.TextColor = UIColor.FromRGB(44, 44, 44);
			labelTaxiHailNetworkEnabled.BackgroundColor = UIColor.Clear;
			labelTaxiHailNetworkEnabled.Font = UIFont.FromName(FontName.HelveticaNeueLight, 32 / 2);

			// Horizontal line bellow master notification toggle
			var enabledToggleSeparator = Line.CreateHorizontal(0, labelTaxiHailNetworkEnabled.Superview.Frame.Height, this.View.Frame.Width, UIColor.LightGray, 1f);
			enabledToggleSeparator.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			this.View.AddSubview(enabledToggleSeparator);

			var set = this.CreateBindingSet<UserTaxiHailNetworkSettingsView, UserTaxiHailNetworkSettingsViewModel> ();

			set.Bind(source)
				.For(v => v.ItemsSource)
				.To(vm => vm.UserTaxiHailNetworkSettings);
			set.Bind(switchTaxiHailNetworkEnabled)
				.For (v => v.On)
				.To (vm => vm.IsTaxiHailNetworkEnabled);
			set.Bind (tableTaxiHailNetworkSettings)
				.For (v => v.Hidden)
				.To (vm => vm.IsTaxiHailNetworkEnabled)
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