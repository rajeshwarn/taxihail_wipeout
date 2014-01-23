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

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	public partial class BookRatingView : BaseViewController<BookRatingViewModel>
	{
		public BookRatingView() 
			: base("BookRatingView", null)
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

			View.BackgroundColor = UIColor.FromRGB (239,239,239);

			FlatButtonStyle.Green.ApplyTo(submitRatingBtn);
            submitRatingBtn.SetTitle(Localize.GetValue("Submit"), UIControlState.Normal);

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

            set.Bind(submitRatingBtn)
                .For("TouchUpInside")
                .To(vm => vm.RateOrder);
            set.Bind(submitRatingBtn)
                .For(v => v.Hidden)
                .To(vm => vm.CanRating)
                .WithConversion("BoolInverter");
            set.Bind(submitRatingBtn)
                .For(v => v.Enabled)
                .To(vm => vm.CanSubmit);

            set.Bind(source)
                .For(v => v.ItemsSource)
                .To(vm => vm.RatingList);

            set.Apply();
		}
	}
}

