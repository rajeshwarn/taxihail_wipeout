using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CrossUI.Touch.Dialog.Elements;
using CrossUI.Touch.Dialog;

namespace apcurium.MK.Booking.Mobile.Client.MonoTouchDialog
{
 
    public class NavigationElement : RootElement
    {
		private readonly Action _navigate;

		public NavigationElement(string caption, Action navigate ) : base (caption)
        {
			_navigate = navigate;
        }

       
		protected override UITableViewCell GetCellImpl(UITableView tv)
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

		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			_navigate();
		}
    }
}
