using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class LocationListView : BaseViewController<LocationListViewModel>
	{
		const string CellId = "LocationCell";
		const string CellBindingText = @"
                   FirstLine Address.FriendlyName;
                   SecondLine Address.FullAddress;
                   ShowRightArrow ShowRightArrow;
                   ShowPlusSign ShowPlusSign;
                   Icon Icon
				";

        public LocationListView () : base ("LocationListView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = Localize.GetValue ("View_MyLocations");
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

			tableLocations.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
			tableLocations.BackgroundColor = UIColor.Clear;
			tableLocations.SeparatorColor = UIColor.Clear;
            tableLocations.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            var source = new BindableTableViewSource (
	             tableLocations, 
	             UITableViewCellStyle.Subtitle, 
				 new NSString (CellId), 
	             CellBindingText, 
	             UITableViewCellAccessory.None
             );
			source.CellCreator = CellCreator;
			tableLocations.Source = source;

            var set = this.CreateBindingSet<LocationListView, LocationListViewModel> ();

			set.Bind(source)
				.For(v => v.ItemsSource)
				.To(vm => vm.AllAddresses);
			set.Bind(source)
				.For(v => v.SelectedCommand)
				.To(vm => vm.NavigateToLocationDetailPage);

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

