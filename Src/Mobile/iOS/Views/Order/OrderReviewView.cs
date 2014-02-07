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
        NSLayoutConstraint _heightConstraint;

        public OrderReviewView (IntPtr handle) : base(handle)
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            _heightConstraint = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, 
                NSLayoutRelation.Equal, 
                null, 
                NSLayoutAttribute.NoAttribute, 
                1.0f, 44.0f);

            this.AddConstraint(_heightConstraint);

            var bindings = this.CreateInlineBindingTarget<OrderReviewViewModel>();

            var name = new StyledStringElement(null,null, UITableViewCellStyle.Default)
                    .Bind(bindings, cell => cell.Caption, vm => vm.Settings.Name);
            name.BackgroundColor = UIColor.Clear;
            name.Font = UIFont.FromName(FontName.HelveticaNeueMedium, 38 / 2);
            name.TextColor = UIColor.Black;
            name.Image = UIImage.FromBundle("account");

            var section = new Section{ name  };

            var root = new RootElement(){ section };

            var dialogViewController = new DialogViewController(UITableViewStyle.Plain, root, false);
            var tableView = dialogViewController.TableView;
            tableView.Bounces = false;
            tableView.ScrollEnabled = false;
            tableView.UserInteractionEnabled = false;
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.BackgroundColor =  UIColor.Clear;           
            tableView.Frame = new RectangleF(2, 2, this.Frame.Width - 4, 300);
            AddSubview(tableView);

            _heightConstraint.Constant = 300;
        }
    }
}

