using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	public partial class BookRatingView : BaseViewController<BookRatingViewModel>
	{
        public BookRatingView() : base("BookRatingView", null)
		{
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationItem.HidesBackButton = false;
			NavigationItem.Title = Localize.GetValue("View_BookRating");
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);

            var btnSubmit = new FlatButton(new RectangleF(8f, BottomPadding, 304f, 41f));
			FlatButtonStyle.Green.ApplyTo(btnSubmit);
			btnSubmit.SetTitle(Localize.GetValue("Submit"), UIControlState.Normal);

            if (ViewModel.CanRate)
            {
                // since we can't have dynamic height of tableview, we put a scrollable tableview with a footer containing the submit button
                var footerView = new UIView(new RectangleF(0f, 0f, 320f, btnSubmit.Frame.Height + BottomPadding * 2));
                footerView.AddSubview(btnSubmit);
                ratingTableView.TableFooterView = footerView;
            }

			var source = new MvxActionBasedTableViewSource(
				ratingTableView,
				UITableViewCellStyle.Default,
				BookRatingCell.Identifier ,
				BookRatingCell.BindingText,
				UITableViewCellAccessory.None);
			source.CellCreator = (tableView, indexPath, item) =>
			{
				return BookRatingCell.LoadFromNib(tableView);
			};
			ratingTableView.Source = source;

            var set = this.CreateBindingSet<BookRatingView, BookRatingViewModel>();

			 set.Bind(btnSubmit)
                .For("TouchUpInside")
                .To(vm => vm.RateOrder);
            set.Bind(btnSubmit)
                .For(v => v.Hidden)
                .To(vm => vm.CanRate)
                .WithConversion("BoolInverter");

            set.Bind(source)
                .For(v => v.ItemsSource)
                .To(vm => vm.RatingList);

            set.Apply();
		}
	}
}

