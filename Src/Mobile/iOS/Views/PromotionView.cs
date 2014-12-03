using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class PromotionView : BaseViewController<PromotionViewModel>
    {
        public PromotionView() : base("PromotionView", null)
        {
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.Title = Localize.GetValue ("View_Promotions");

            ChangeThemeOfBarStyle();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			
            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

            tblPromotions.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tblPromotions.BackgroundColor = UIColor.Clear;
            tblPromotions.SeparatorColor = UIColor.Clear;
            tblPromotions.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tblPromotions.DelaysContentTouches = false;

            lblNoPromotions.Text = Localize.GetValue("PromotionViewNoPromotionLabel");
            lblNoPromotions.Hidden = true;

            var tableViewSource = new PromotionTableViewSource(tblPromotions);
            tblPromotions.Source = tableViewSource;

            var set = this.CreateBindingSet<PromotionView, PromotionViewModel> ();

            set.Bind(tblPromotions)
                .For(v => v.Hidden)
                .To(vm => vm.HasPromotions)
                .WithConversion("BoolInverter");

            set.Bind(lblNoPromotions)
                .For(v => v.Hidden)
                .To(vm => vm.HasPromotions);

            set.Bind(tableViewSource)
                .For(v => v.ItemsSource)
                .To(vm => vm.ActivePromotions);

            set.Apply ();
        }
    }

    public class PromotionTableViewSource : AmpTableViewSource<PromotionCell>
    {
        public PromotionTableViewSource(UITableView tableView) : base(tableView)
        {
        }

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
        {
            var cell = base.GetOrCreateCellFor(tableView, indexPath, item);

            ((PromotionCell)cell).HideBottomBar = tableView.IsLastCell(indexPath);

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);

            var item = (PromotionItemViewModel)GetItemAt(indexPath);
            item.SelectPromotion.ExecuteIfPossible();
        }
    }
}

