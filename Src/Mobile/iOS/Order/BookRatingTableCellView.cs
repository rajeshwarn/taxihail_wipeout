
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class BookRatingTableCellView : MvxBindableTableViewCell
	{
		public BookRatingTableCellView (string bindingText) : base (bindingText, UITableViewCellStyle.Default, new NSString("BookRatingTableCellView"), UITableViewCellAccessory.None )
		{
		}

		public string RatingTitle {
			get {
				return titleText.Text;
			}
			set {
				titleText.Text = value;
			}
		}
	}
}

