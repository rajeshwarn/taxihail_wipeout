using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	public partial class BookRatingView : MvxViewController
	{
		public BookRatingView() 
			: base("BookRatingView", null)
		{
		}
		
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
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
					
			this.AddBindings(new Dictionary<object, string> {
				{ submitRatingBtn, "{'TouchUpInside':{'Path':'RateOrder'}, 'Hidden':{'Path': 'CanRating', 'Converter':'BoolInverter'}, 'Enabled': {'Path': 'CanSubmit'}}"},                
				{ source, "{'ItemsSource':{'Path':'RatingList'}}" }
			});

            ratingTableView.BackgroundColor = UIColor.Clear;
            ratingTableView.BackgroundView = new UIView();
			ratingTableView.Source = source;
			ratingTableView.ReloadData();

            View.ApplyAppFont ();

		}
	}
}

