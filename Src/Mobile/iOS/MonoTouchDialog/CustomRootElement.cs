using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
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

       
        public override UITableViewCell GetCell(UITableView tv)
        {
            var cell =  base.GetCell(tv);
            cell.BackgroundColor = UIColor.Clear;
            return cell;
        }
        protected override void PrepareDialogViewController (UIViewController dvc)
        {
            dvc.View.BackgroundColor = UIColor.Clear;

            dvc.View.BackgroundColor = UIColor.Clear; // UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

            //((UITableViewController)dvc).TableView.BackgroundColor = UIColor.Clear;
            base.PrepareDialogViewController (dvc);

           
        }
    }
}
