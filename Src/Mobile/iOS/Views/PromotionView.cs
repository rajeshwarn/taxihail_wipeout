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
        private NSIndexPath _selectedIndexPath;

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

            if (_selectedIndexPath == null || _selectedIndexPath.Row != indexPath.Row)
            {
                // selected a new cell, collapse previous if it exists and expand new one
                if (_selectedIndexPath != null)
                {
                    var previousCell = (PromotionCell)tableView.CellAt(_selectedIndexPath);
                    previousCell.IsExpanded = false;
                }

                var cell = (PromotionCell)tableView.CellAt(indexPath);
                cell.IsExpanded = true;

                _selectedIndexPath = indexPath;
            }
            else
            {
                // selected the same cell, collapse it
                var cell = (PromotionCell)tableView.CellAt(indexPath);
                cell.IsExpanded = false;

                _selectedIndexPath = null;
            }

            tableView.BeginUpdates();
            tableView.EndUpdates();
        }

        public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (_selectedIndexPath == null || _selectedIndexPath.Row != indexPath.Row)
            {
                return PromotionCell.Height;
            }

            var item = (PromotionItemViewModel)GetItemAt(indexPath);

            float nonDynamicHeight = 15 /* Top padding */
                + 5                     /* Vertical spacing between title and description */
                + 12                    /* Vertical spacing between description and button */
                + 41                    /* Button height */
                + 10;                   /* Bottom padding */

            var maxSizeForTitle = new SizeF(200, 0);
            var maxSizeForDescription = new SizeF(new UIEdgeInsets(0, 15, 0, 15).InsetRect(tableView.Bounds).Width, 0);

            var titleTextSize = tableView.GetSizeThatFits(item.Name, UIFont.FromName(FontName.HelveticaNeueBold, 28 / 2), maxSizeForTitle);
            var descriptionTextSize = tableView.GetSizeThatFits(item.Description, UIFont.FromName(FontName.HelveticaNeueLight, 28 / 2), maxSizeForDescription);

            return nonDynamicHeight + titleTextSize.Height + descriptionTextSize.Height;
        }
    }
}

