using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
 
    public class NavigationElement : RootElement
    {
		private Action _navigate;

		public NavigationElement(string caption, Action navigate ) : base (caption)
        {
			_navigate = navigate;
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

            dvc.View.BackgroundColor = UIColor.Clear;


            ((UITableViewController)dvc).TableView.BackgroundColor = UIColor.Clear;
            ((UITableViewController)dvc).TableView.BackgroundView = new UIView{ BackgroundColor = UIColor.Clear};

            base.PrepareDialogViewController (dvc);
			   
        }

		public override void Selected (DialogViewController dvc, UITableView tableView, MonoTouch.Foundation.NSIndexPath path)
		{
			_navigate();
		}
    }
}
