using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using CrossUI.Touch.Dialog.Elements;
using CrossUI.Touch.Dialog;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
    public partial class OrderReviewView : OverlayView
    {
        public OrderReviewView (IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Green;

            var bindings = this.CreateInlineBindingTarget<OrderReviewViewModel>();
            var name = new StyledStringElement(null,null, UITableViewCellStyle.Default)
                    .Bind(bindings, cell => cell.Caption, vm => vm.Settings.Name);
            name.BackgroundColor = UIColor.Clear;
            name.Font = UIFont.FromName(FontName.HelveticaNeueMedium, 38 / 2);
            name.TextColor = UIColor.Black;
            name.Image = UIImage.FromBundle("account");

            var section = new Section
                {
                    name                
                };

            var root = new RootElement(){ section };

            var tableView = new DialogViewController(root, false).TableView;
            tableView.Bounces = false;
            tableView.ScrollEnabled = false;
            tableView.UserInteractionEnabled = false;
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.BackgroundColor =  UIColor.Red;
            tableView.SectionHeaderHeight = 0;
            tableView.SectionFooterHeight = 0;
            tableView.Frame = tableView.Frame.SetWidth(this.Frame.Width - 10);
            tableView.Frame = tableView.Frame.SetHeight(this.Frame.Height - 10);
            AddSubview(tableView);
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("OrderReviewView", null);
            AddSubview((UIView)nib.Instantiate (this, null)[0]);

            Initialize();
        }
    }
}

