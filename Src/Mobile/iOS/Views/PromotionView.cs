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

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class PromotionView : BaseViewController<PromotionViewModel>
    {
        const string CellId = "HistoryCell";
        const string CellBindingText = @"
                   FirstLine Name;
                   SecondLine Description
                "; 

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

            var source = new BindableTableViewSource (
                tblPromotions, 
                UITableViewCellStyle.Subtitle, 
                new NSString (CellId), 
                CellBindingText, 
                UITableViewCellAccessory.None
            );
            source.CellCreator = CellCreator;
            tblPromotions.Source = source;

            var set = this.CreateBindingSet<PromotionView, PromotionViewModel> ();

            set.Bind(tblPromotions)
                .For(v => v.Hidden)
                .To(vm => vm.HasPromotions)
                .WithConversion("BoolInverter");

            set.Bind(lblNoPromotions)
                .For(v => v.Hidden)
                .To(vm => vm.HasPromotions);

            set.Bind(source)
                .For(v => v.ItemsSource)
                .To(vm => vm.ActivePromotions);
            set.Bind(source)
                .For(v => v.SelectedCommand)
                .To(vm => vm.SelectPromotion);

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

