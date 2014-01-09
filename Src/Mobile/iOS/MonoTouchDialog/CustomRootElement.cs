using System;
using MonoTouch.UIKit;
using CrossUI.Touch.Dialog.Elements;

namespace apcurium.MK.Booking.Mobile.Client.MonoTouchDialog
{
 
    public class CustomRootElement : RootElement
    {
        public CustomRootElement(string caption) : base (caption)
        {
        }

        public CustomRootElement(string caption, Func<RootElement, UIViewController> createOnSelected) : base (caption, createOnSelected)
        {
        }

        public CustomRootElement(string caption, int section, int element) : base (caption, section, element)
        {
        }

        public CustomRootElement(string caption, Group group) : base (caption, group)
        {
        }



       
        protected override UITableViewCell GetCellImpl (UITableView tv)
        {
            var cell =  base.GetCellImpl(tv);
            cell.BackgroundColor = UIColor.Clear;
            return cell;
        }

        protected override void PrepareDialogViewController (UIViewController dvc)
        {
            dvc.View.BackgroundColor = UIColor.Clear;

            ((UITableViewController)dvc).TableView.BackgroundColor = UIColor.Clear;
            ((UITableViewController)dvc).TableView.BackgroundView = new UIView{ BackgroundColor = UIColor.Clear};

            base.PrepareDialogViewController (dvc);

        }
    }
}
