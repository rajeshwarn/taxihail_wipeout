using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	public partial class BookRatingView : MvxBindingTouchViewController<BookRatingViewModel>
	{
		public BookRatingView() 
			: base(new MvxShowViewModelRequest<BookViewModel>( null, true, new MvxRequestedBy()   ) )
		{
		}
		
		public BookRatingView(MvxShowViewModelRequest request) 
			: base(request)
		{
		}
		
		public BookRatingView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{
		}
		
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
			submitRatingBtn.SetTitle(Resources.Submit, UIControlState.Normal);

			var source = new MvxActionBasedBindableTableViewSource(
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

