using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
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

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = Localize.GetValue("View_BookRating");

			ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (ViewModel.CanUserLeaveScreen ()) 
			{
				NavigationItem.HidesBackButton = false;
			}else 
			{
				NavigationItem.HidesBackButton = true;
			}

            View.BackgroundColor = UIColor.FromRGB (242, 242, 242);
            ratingTableView.BackgroundColor = UIColor.Clear;
            ratingTableView.SeparatorColor = UIColor.Clear;
            ratingTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            ratingTableView.DelaysContentTouches = false;

            var btnSubmit = new FlatButton(new RectangleF(8f, BottomPadding, 304f, 41f));
			FlatButtonStyle.Green.ApplyTo(btnSubmit);
			btnSubmit.SetTitle(Localize.GetValue("Submit"), UIControlState.Normal);

			ViewModel.PropertyChanged += async (sender, e) => {
				if (e.PropertyName == "CanRate") {
					ShowDoneButton ();
				}
			};

			var source = new MvxActionBasedTableViewSource(
				ratingTableView,
				UITableViewCellStyle.Default,
				BookRatingCell.Identifier ,
				BookRatingCell.BindingText,
				UITableViewCellAccessory.None);
			source.CellCreator = (tableView, indexPath, item) =>
			{
                var cell = BookRatingCell.LoadFromNib(tableView);
                cell.RemoveDelay();
                return cell;
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


		private void ShowDoneButton()
		{


			NavigationItem.RightBarButtonItem =  !ViewModel.CanRate ? null : new UIBarButtonItem (Localize.GetValue ("Done"), UIBarButtonItemStyle.Bordered, async (o, e) => {  
			
				if (ViewModel.CanRate ) {
					ViewModel.RateOrder.Execute(null);

				}
			});
		}

	}
}

