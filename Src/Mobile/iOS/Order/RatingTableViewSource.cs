using System;
using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class RatingTableViewSource: MvxActionBasedBindableTableViewSource
	{
		public RatingTableViewSource (UITableView tableView, NSString cellIdentifier, string bindingText) : base(tableView, UITableViewCellStyle.Default, cellIdentifier, bindingText, UITableViewCellAccessory.None)
		{

		}

		protected override UITableViewCell GetOrCreateCellFor (UITableView tableView, NSIndexPath indexPath, object item)
		{
			return base.GetOrCreateCellFor (tableView, indexPath, item);
		}
	}
}

