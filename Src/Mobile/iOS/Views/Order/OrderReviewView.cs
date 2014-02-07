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
    [Register("OrderReviewView")]
    public class OrderReviewView : OverlayView
    {
        public OrderReviewView (IntPtr handle) : base(handle)
        {
            Initialize();
        }

        private void Initialize()
        {
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

            var dialogViewController = new DialogViewController(root, false);
            var tableView = dialogViewController.TableView;
            tableView.Bounces = false;
            tableView.ScrollEnabled = false;
            tableView.UserInteractionEnabled = false;
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.BackgroundColor =  UIColor.Clear; 
            tableView.SectionFooterHeight = 0;        
            tableView.Frame = new RectangleF(2, 2, this.Frame.Width - 4, this.Frame.Height - 4);
            AddSubview(tableView);
        }
    }
}

